using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using ClickyWindows.Models;
using ClickyWindows.WindowsAPI;

namespace ClickyWindows.Services;

/// <summary>
/// Captures screenshots of all connected displays as JPEG byte arrays.
/// Uses GDI BitBlt for cross-monitor capture (multi-monitor aware).
///
/// Equivalent to macOS CompanionScreenCaptureUtility (ScreenCaptureKit).
/// </summary>
public sealed class ScreenCaptureService : IDisposable
{
    private const int MaxDimension = 1280;
    private const float JpegQuality = 0.80f;

    /// <summary>
    /// Captures all monitors as JPEG byte arrays, sorted so the cursor
    /// screen comes first. Each capture is labeled for the AI.
    /// </summary>
    public async Task<List<ScreenCaptureResult>> CaptureAllScreensAsync(CancellationToken ct = default)
    {
        var results = new List<ScreenCaptureResult>();

        await Task.Run(() =>
        {
            NativeMethods.GetCursorPos(out var cursorPoint);

            // Get all monitor bounds
            var monitors = new List<(Rectangle bounds, string deviceName, bool isPrimary)>
            {
                // Dummy entry removed - we populate from EnumDisplayMonitors
            };

            NativeMethods.EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero,
                (IntPtr hMonitor, IntPtr hdc, ref NativeMethods.RECT rect, IntPtr data) =>
            {
                var mi = new NativeMethods.MONITORINFOEX
                {
                    cbSize = (uint)Marshal.SizeOf<NativeMethods.MONITORINFOEX>()
                };
                if (NativeMethods.GetMonitorInfo(hMonitor, ref mi))
                {
                    var r = mi.rcMonitor;
                    monitors.Add((
                        new Rectangle(r.left, r.top, r.right - r.left, r.bottom - r.top),
                        mi.szDevice,
                        (mi.dwFlags & 1) != 0));
                }
                return true;
            }, IntPtr.Zero);

            // Sort: cursor screen first, then primary, then others
            var sorted = monitors
                .Select(m => (m.bounds, m.deviceName, m.isPrimary,
                    isCursor: m.bounds.Contains(cursorPoint.x, cursorPoint.y)))
                .OrderByDescending(m => m.isCursor)
                .ThenByDescending(m => m.isPrimary)
                .ToList();

            for (int i = 0; i < sorted.Count; i++)
            {
                if (ct.IsCancellationRequested) break;

                var (bounds, deviceName, isPrimary, isCursor) = sorted[i];
                try
                {
                    var result = CaptureSingleScreen(bounds, isCursor, isPrimary, i + 1, sorted.Count);
                    if (result != null)
                        results.Add(result);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"📸 Error capturing screen {deviceName}: {ex.Message}");
                }
            }
        }, ct);

        return results;
    }

    /// <summary>
    /// Captures a single monitor using GDI BitBlt, resizes to max 1280px,
    /// and returns as JPEG byte array with metadata.
    /// </summary>
    private static ScreenCaptureResult? CaptureSingleScreen(
        Rectangle bounds, bool isCursorScreen, bool isPrimary,
        int screenNumber, int totalMonitors)
    {
        using var screenBmp = new Bitmap(bounds.Width, bounds.Height);
        using var screenG = Graphics.FromImage(screenBmp);

        IntPtr hdc = screenG.GetHdc();
        IntPtr desktopHdc = NativeMethods.GetDC(IntPtr.Zero);

        try
        {
            NativeMethods.BitBlt(hdc, 0, 0, bounds.Width, bounds.Height,
                desktopHdc, bounds.X, bounds.Y,
                NativeMethods.SRCCOPY | NativeMethods.CAPTUREBLT);
        }
        finally
        {
            screenG.ReleaseHdc(hdc);
            NativeMethods.ReleaseDC(IntPtr.Zero, desktopHdc);
        }

        // Resize to max 1280px on the longest edge
        int newWidth, newHeight;
        if (bounds.Width >= bounds.Height)
        {
            newWidth = Math.Min(MaxDimension, bounds.Width);
            newHeight = (int)((double)newWidth / bounds.Width * bounds.Height);
        }
        else
        {
            newHeight = Math.Min(MaxDimension, bounds.Height);
            newWidth = (int)((double)newHeight / bounds.Height * bounds.Width);
        }

        using var resizedBmp = new Bitmap(newWidth, newHeight);
        using (var g = Graphics.FromImage(resizedBmp))
        {
            g.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic;
            g.DrawImage(screenBmp, 0, 0, newWidth, newHeight);
        }

        // Encode as JPEG
        using var ms = new MemoryStream();
        var encoderParams = new EncoderParameters(1);
        encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, (long)(JpegQuality * 100));

        var jpegCodec = GetJpegCodec();
        if (jpegCodec == null) return null;

        resizedBmp.Save(ms, jpegCodec, encoderParams);
        byte[] imageData = ms.ToArray();

        // Build descriptive label for the AI
        string label;
        if (totalMonitors == 1)
        {
            label = "user's screen (cursor is here)";
        }
        else if (isCursorScreen)
        {
            label = $"screen {screenNumber} of {totalMonitors} — cursor is on this screen (primary focus)";
        }
        else
        {
            string suffix = isPrimary ? " (primary display)" : "";
            label = $"screen {screenNumber} of {totalMonitors}{suffix} — secondary screen";
        }

        return new ScreenCaptureResult
        {
            ImageData = imageData,
            Label = label,
            IsCursorScreen = isCursorScreen,
            DisplayWidthInPoints = bounds.Width,
            DisplayHeightInPoints = bounds.Height,
            DisplayFrame = new IntRect(bounds.X, bounds.Y, bounds.Width, bounds.Height),
            ScreenshotWidthInPixels = newWidth,
            ScreenshotHeightInPixels = newHeight,
        };
    }

    private static ImageCodecInfo? GetJpegCodec()
    {
        return ImageCodecInfo.GetImageEncoders().FirstOrDefault(c => c.FormatID == ImageFormat.Jpeg.Guid);
    }

    public void Dispose() { }
}
