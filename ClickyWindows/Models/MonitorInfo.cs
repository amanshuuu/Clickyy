namespace ClickyWindows.Models;

/// <summary>
/// Represents a connected display with its bounds and DPI info.
/// Used to create one overlay window per monitor and for coordinate mapping.
/// </summary>
public class MonitorInfo
{
    public string DeviceName { get; set; } = string.Empty;
    public int Left { get; set; }
    public int Top { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public bool IsPrimary { get; set; }
    public float DpiScaleX { get; set; } = 1.0f;
    public float DpiScaleY { get; set; } = 1.0f;
    public IntPtr HMonitor { get; set; }

    /// <summary>Physical pixel bounds of the entire monitor.</summary>
    public System.Windows.Rect Bounds => new(Left, Top, Width, Height);

    /// <summary>The label sent to the AI identifying this screen.</summary>
    public string GetLabel(int totalMonitors) => totalMonitors == 1
        ? "user's screen (cursor is here)"
        : (IsPrimary
            ? $"screen 1 of {totalMonitors} — cursor is on this screen (primary focus)"
            : $"screen {Array.IndexOf(GetAllMonitors(), this) + 1} of {totalMonitors} — secondary screen");

    /// <summary>
    /// Returns the DPI for this monitor (uses Windows 8.1+ GetDpiForMonitor API).
    /// Falls back to 96 DPI (1.0 scale) if the API is unavailable.
    /// </summary>
    public (float dpiX, float dpiY) GetDpi()
    {
        try
        {
            uint dpiX = 96, dpiY = 96;
            int hr = WindowsAPI.NativeMethods.GetDpiForMonitor(
                HMonitor, WindowsAPI.NativeMethods.MonitorDpiType.MDT_EFFECTIVE_DPI,
                out dpiX, out dpiY);
            if (hr == 0) // S_OK
            {
                return (dpiX, dpiY);
            }
        }
        catch { }
        return (96, 96);
    }

    /// <summary>Returns the DPI scale factor (e.g., 1.25 for 125%).</summary>
    public float GetDpiScaleFactor()
    {
        var (dpiX, _) = GetDpi();
        return dpiX / 96.0f;
    }

    private static MonitorInfo[]? _allMonitors;
    internal static MonitorInfo[] GetAllMonitors()
    {
        _allMonitors ??= Array.Empty<MonitorInfo>();
        return _allMonitors;
    }

    internal static void SetAllMonitors(MonitorInfo[] monitors) => _allMonitors = monitors;
}
