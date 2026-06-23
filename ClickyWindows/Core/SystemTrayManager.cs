using System.Windows.Interop;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Diagnostics;
using ClickyWindows.WindowsAPI;

namespace ClickyWindows.Core;

/// <summary>
/// Manages the Windows system tray icon (equivalent to macOS NSStatusItem).
/// Shows a clickable blue triangle icon. Clicking opens the companion panel.
/// Right-click shows a context menu with settings, onboarding, and Quit.
/// </summary>
public sealed class SystemTrayManager : IDisposable
{
    private readonly Window _messageWindow;
    private IntPtr _hWnd;
    private bool _disposed;

    public event EventHandler? TrayIconClicked;
    public event EventHandler? TrayIconDoubleClicked;
    public event EventHandler? QuitRequested;
    public event EventHandler? OnboardingRequested;

    /// <summary>
    /// The message-only window used for tray icon callbacks.
    /// </summary>
    public IntPtr Handle => _hWnd;

    public SystemTrayManager(Window messageWindow)
    {
        _messageWindow = messageWindow;
        _hWnd = new WindowInteropHelper(messageWindow).Handle;
    }

    /// <summary>
    /// Creates or updates the system tray icon.
    /// The icon is drawn programmatically (blue triangle, same shape as the cursor).
    /// </summary>
    public void CreateTrayIcon()
    {
        var hWnd = _hWnd;

        // Create the icon programmatically: a small blue triangle
        var icon = CreateTriangleIcon();

        var nid = new NativeMethods.NOTIFYICONDATA
        {
            cbSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf<NativeMethods.NOTIFYICONDATA>(),
            hWnd = hWnd,
            uID = 1,
            uFlags = NativeMethods.NIF_MESSAGE | NativeMethods.NIF_ICON | NativeMethods.NIF_TIP | NativeMethods.NIF_SHOWTIP,
            uCallbackMessage = NativeMethods.WM_TRAYICON,
            hIcon = (icon != null ? icon.Handle : IntPtr.Zero),
            szTip = "Clicky — Press Ctrl+Alt to talk",
            uTimeoutOrVersion = 4, // NOTIFYICON_VERSION_4
        };

        NativeMethods.Shell_NotifyIcon(NativeMethods.NIM_ADD, ref nid);
        NativeMethods.Shell_NotifyIcon(NativeMethods.NIM_SETVERSION, ref nid);

        // Hook the WPF window's WndProc for tray messages
        var source = HwndSource.FromHwnd(hWnd);
        source?.AddHook(WndProc);
    }

    /// <summary>
    /// Shows a notification balloon from the tray icon.
    /// </summary>
    public void ShowNotification(string title, string text)
    {
        var nid = new NativeMethods.NOTIFYICONDATA
        {
            cbSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf<NativeMethods.NOTIFYICONDATA>(),
            hWnd = _hWnd,
            uID = 1,
            uFlags = NativeMethods.NIF_INFO,
            szInfo = text,
            szInfoTitle = title,
            dwInfoFlags = 0,
            uTimeoutOrVersion = 5000,
        };
        NativeMethods.Shell_NotifyIcon(NativeMethods.NIM_MODIFY, ref nid);
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == NativeMethods.WM_TRAYICON)
        {
            uint uMsg = (uint)lParam;

            switch (uMsg)
            {
                case 0x0202: // WM_LBUTTONUP
                    TrayIconClicked?.Invoke(this, EventArgs.Empty);
                    handled = true;
                    break;

                case 0x0203: // WM_LBUTTONDBLCLK
                    TrayIconDoubleClicked?.Invoke(this, EventArgs.Empty);
                    handled = true;
                    break;

                case 0x0205: // WM_RBUTTONUP
                    ShowContextMenu();
                    handled = true;
                    break;
            }
        }

        return IntPtr.Zero;
    }

    private void ShowContextMenu()
    {
        var contextMenu = new System.Windows.Forms.ContextMenuStrip();
        contextMenu.BackColor = System.Drawing.Color.FromArgb(23, 25, 24);
        contextMenu.ForeColor = System.Drawing.Color.FromArgb(236, 238, 237);

        // Style helper
        void AddItem(string text, System.Drawing.Image? icon, EventHandler handler)
        {
            var item = new System.Windows.Forms.ToolStripMenuItem(text, icon, handler);
            contextMenu.Items.Add(item);
        }

        AddItem("Open Clicky", null, (_, _) => TrayIconClicked?.Invoke(this, EventArgs.Empty));
        AddItem("Run Onboarding Demo", null, (_, _) => OnboardingRequested?.Invoke(this, EventArgs.Empty));

        contextMenu.Items.Add(new System.Windows.Forms.ToolStripSeparator());

        AddItem("Get Free API Key at OpenRouter", null, (_, _) =>
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://openrouter.ai",
                    UseShellExecute = true
                });
            }
            catch { }
        });

        contextMenu.Items.Add(new System.Windows.Forms.ToolStripSeparator());

        AddItem("Quit", null, (_, _) => QuitRequested?.Invoke(this, EventArgs.Empty));

        // Show at cursor position
        var cursorPos = System.Windows.Forms.Cursor.Position;
        contextMenu.Show(cursorPos);
    }

    /// <summary>
    /// Creates a simple blue triangle icon programmatically.
    /// Matches the macOS menu bar icon shape (rotated 35 degrees).
    /// </summary>
    private static System.Drawing.Icon? CreateTriangleIcon()
    {
        const int size = 18;
        var bitmap = new System.Drawing.Bitmap(size, size);
        using (var g = System.Drawing.Graphics.FromImage(bitmap))
        {
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.Clear(System.Drawing.Color.Transparent);

            var cx = size / 2.0f;
            var cy = size / 2.0f;
            var triangleSize = size * 0.7f;
            var height = triangleSize * (float)Math.Sqrt(3.0) / 2.0f;

            var top = new System.Drawing.PointF(cx, cy - height / 1.5f);
            var bottomLeft = new System.Drawing.PointF(cx - triangleSize / 2, cy + height / 3);
            var bottomRight = new System.Drawing.PointF(cx + triangleSize / 2, cy + height / 3);

            // Rotate 35 degrees
            var angle = 35.0 * Math.PI / 180.0;
            System.Drawing.PointF Rotate(System.Drawing.PointF pt)
            {
                var dx = pt.X - cx;
                var dy = pt.Y - cy;
                var cosA = (float)Math.Cos(angle);
                var sinA = (float)Math.Sin(angle);
                return new System.Drawing.PointF(cx + cosA * dx - sinA * dy, cy + sinA * dx + cosA * dy);
            }

            var pts = new[] { Rotate(top), Rotate(bottomLeft), Rotate(bottomRight) };

            using var brush = new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(96, 165, 250)); // Blue-400
            g.FillPolygon(brush, pts);
        }

        var hIcon = bitmap.GetHicon();
        return System.Drawing.Icon.FromHandle(hIcon);
    }

    public void RemoveTrayIcon()
    {
        var nid = new NativeMethods.NOTIFYICONDATA
        {
            cbSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf<NativeMethods.NOTIFYICONDATA>(),
            hWnd = _hWnd,
            uID = 1,
        };
        NativeMethods.Shell_NotifyIcon(NativeMethods.NIM_DELETE, ref nid);
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            RemoveTrayIcon();
            _disposed = true;
        }
    }
}
