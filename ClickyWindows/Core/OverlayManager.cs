using System.Diagnostics;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using ClickyWindows.Models;
using ClickyWindows.UI.Overlay;
using ClickyWindows.WindowsAPI;

namespace ClickyWindows.Core;

/// <summary>
/// Manages transparent overlay windows — one per monitor — that host the
/// blue cursor buddy, waveform animation, spinner, and text bubble.
/// Equivalent to macOS OverlayWindowManager.
/// </summary>
public sealed class OverlayManager : IDisposable
{
    private readonly List<OverlayWindow> _overlayWindows = new();
    private bool _hasShownOverlayBefore;
    private bool _isFading;
    public bool HasShownOverlayBefore => _hasShownOverlayBefore;

    public void ShowOverlay()
    {
        // If currently fading out, cancel it
        _isFading = false;

        // Close any existing overlay windows
        HideOverlay();

        var monitors = GetMonitorInfos();
        MonitorInfo.SetAllMonitors(monitors);

        foreach (var monitor in monitors)
        {
            var overlayWindow = new OverlayWindow(monitor);
            overlayWindow.Show();
            _overlayWindows.Add(overlayWindow);
        }

        _hasShownOverlayBefore = true;
        Debug.WriteLine($"🖥️ Overlay shown on {monitors.Length} monitor(s)");
    }

    public void HideOverlay()
    {
        _isFading = false;
        foreach (var w in _overlayWindows)
        {
            try { w.Close(); } catch { }
        }
        _overlayWindows.Clear();
    }

    /// <summary>
    /// Fades out overlay windows smoothly over the given duration,
    /// then closes them. Safe to call multiple times.
    /// </summary>
    public void FadeOutAndHide(double durationSeconds = 0.4)
    {
        if (_isFading) return;
        if (_overlayWindows.Count == 0) return;

        _isFading = true;
        var windowsToFade = _overlayWindows.ToList();
        _overlayWindows.Clear();

        var fadeTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(durationSeconds / 20)
        };

        int steps = 20;
        int currentStep = 0;

        fadeTimer.Tick += (s, e) =>
        {
            if (!_isFading)
            {
                // Cancelled — close immediately
                fadeTimer.Stop();
                foreach (var w in windowsToFade)
                {
                    try { w.Close(); } catch { }
                }
                return;
            }

            currentStep++;
            double opacity = 1.0 - (currentStep / (double)steps);
            foreach (var w in windowsToFade)
                w.Opacity = Math.Max(0, opacity);

            if (currentStep >= steps)
            {
                fadeTimer.Stop();
                _isFading = false;
                foreach (var w in windowsToFade)
                {
                    try { w.Close(); } catch { }
                }
            }
        };
        fadeTimer.Start();
    }

    public void UpdateCursorPosition(Point position)
    {
        foreach (var w in _overlayWindows)
            w.UpdateCursorPosition(position);
    }

    public void SetVoiceState(VoiceState state)
    {
        foreach (var w in _overlayWindows)
            w.SetVoiceState(state);
    }

    public void SetResponseText(string text)
    {
        foreach (var w in _overlayWindows)
            w.SetResponseText(text);
    }

    public void SetAudioLevel(float level)
    {
        foreach (var w in _overlayWindows)
            w.SetAudioLevel(level);
    }

    /// <summary>
    /// Enumerates all monitors and returns their info including DPI.
    /// </summary>
    public static MonitorInfo[] GetMonitorInfos()
    {
        var monitors = new List<MonitorInfo>();

        NativeMethods.EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero,
            (IntPtr hMonitor, IntPtr hdc, ref NativeMethods.RECT rect, IntPtr data) =>
        {
            var mi = new NativeMethods.MONITORINFOEX
            {
                cbSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf<NativeMethods.MONITORINFOEX>()
            };
            if (NativeMethods.GetMonitorInfo(hMonitor, ref mi))
            {
                bool isPrimary = (mi.dwFlags & 1) != 0;
                var monitorInfo = new MonitorInfo
                {
                    DeviceName = mi.szDevice,
                    Left = mi.rcMonitor.left,
                    Top = mi.rcMonitor.top,
                    Width = mi.rcMonitor.right - mi.rcMonitor.left,
                    Height = mi.rcMonitor.bottom - mi.rcMonitor.top,
                    IsPrimary = isPrimary,
                    HMonitor = hMonitor,
                };

                // Populate DPI info
                try
                {
                    uint dpiX = 96, dpiY = 96;
                    int hr = NativeMethods.GetDpiForMonitor(
                        hMonitor, NativeMethods.MonitorDpiType.MDT_EFFECTIVE_DPI,
                        out dpiX, out dpiY);
                    if (hr == 0)
                    {
                        monitorInfo.DpiScaleX = dpiX / 96f;
                        monitorInfo.DpiScaleY = dpiY / 96f;
                    }
                }
                catch { }

                monitors.Add(monitorInfo);
            }
            return true;
        }, IntPtr.Zero);

        return monitors.ToArray();
    }

    public static (MonitorInfo? monitor, int index) GetMonitorAtPoint(Point point)
    {
        var monitors = MonitorInfo.GetAllMonitors();
        for (int i = 0; i < monitors.Length; i++)
        {
            if (monitors[i].Bounds.Contains(point))
                return (monitors[i], i);
        }
        return (null, -1);
    }

    public void Dispose()
    {
        HideOverlay();
    }
}
