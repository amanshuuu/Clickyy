using System.Windows;

namespace ClickyWindows.Models;

/// <summary>
/// Result of capturing one display as a JPEG byte array, with metadata
/// needed for AI coordinate mapping.
/// </summary>
public class ScreenCaptureResult
{
    public byte[] ImageData { get; set; } = Array.Empty<byte>();
    public string Label { get; set; } = string.Empty;
    public bool IsCursorScreen { get; set; }
    public int DisplayWidthInPoints { get; set; }
    public int DisplayHeightInPoints { get; set; }
    public IntRect DisplayFrame { get; set; }
    public int ScreenshotWidthInPixels { get; set; }
    public int ScreenshotHeightInPixels { get; set; }
}

public struct IntRect
{
    public int X;
    public int Y;
    public int Width;
    public int Height;

    public IntRect(int x, int y, int w, int h)
    {
        X = x; Y = y; Width = w; Height = h;
    }

    public bool Contains(int px, int py) =>
        px >= X && px < X + Width && py >= Y && py < Y + Height;
}
