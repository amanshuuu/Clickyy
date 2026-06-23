using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using ClickyWindows.Core;
using ClickyWindows.Models;
using ClickyWindows.WindowsAPI;

namespace ClickyWindows.UI.Overlay;

public class OverlayWindow : Window
{
    private readonly MonitorInfo _monitor;
    private readonly BlueCursorControl _cursorControl;

    public OverlayWindow(MonitorInfo monitor)
    {
        _monitor = monitor;

        Title = "Clicky Overlay";
        WindowStyle = WindowStyle.None;
        AllowsTransparency = true;
        Background = new SolidColorBrush(Colors.Transparent);
        Topmost = true;
        ShowInTaskbar = false;
        ShowActivated = false;
        ResizeMode = ResizeMode.NoResize;
        WindowStartupLocation = WindowStartupLocation.Manual;
        Cursor = null;

        Left = monitor.Left;
        Top = monitor.Top;
        Width = monitor.Width;
        Height = monitor.Height;

        _cursorControl = new BlueCursorControl(monitor);
        Content = _cursorControl;

        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        var hWnd = new WindowInteropHelper(this).Handle;
        if (hWnd != IntPtr.Zero)
        {
            var exStyle = NativeMethods.GetWindowLong(hWnd, NativeMethods.GWL_EXSTYLE);
            NativeMethods.SetWindowLong(hWnd, NativeMethods.GWL_EXSTYLE,
                exStyle | NativeMethods.WS_EX_LAYERED | NativeMethods.WS_EX_TRANSPARENT | NativeMethods.WS_EX_NOACTIVATE | NativeMethods.WS_EX_TOOLWINDOW);

            NativeMethods.SetLayeredWindowAttributes(hWnd, 0, 255, NativeMethods.LWA_ALPHA);
            NativeMethods.SetWindowPos(hWnd, (IntPtr)NativeMethods.HWND_TOPMOST,
                0, 0, 0, 0,
                NativeMethods.SWP_NOMOVE | NativeMethods.SWP_NOSIZE | NativeMethods.SWP_NOACTIVATE);
        }
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        var hWnd = new WindowInteropHelper(this).Handle;
        var source = HwndSource.FromHwnd(hWnd);
        source?.AddHook(WndProc);
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == NativeMethods.WM_MOUSEACTIVATE)
        {
            handled = true;
            return (IntPtr)NativeMethods.MA_NOACTIVATE;
        }
        if (msg == NativeMethods.WM_NCHITTEST)
        {
            handled = true;
            return (IntPtr)NativeMethods.HTTRANSPARENT;
        }
        return IntPtr.Zero;
    }

    #region Public API

    public void UpdateCursorPosition(System.Windows.Point position)
    {
        _cursorControl.UpdateCursorPosition(position);
    }

    public void SetVoiceState(VoiceState state)
    {
        _cursorControl.SetVoiceState(state);
    }

    public void SetResponseText(string text)
    {
        _cursorControl.SetResponseText(text);
    }

    public void SetAudioLevel(float level)
    {
        _cursorControl.SetAudioLevel(level);
    }

    #endregion
}
