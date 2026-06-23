using System.Windows.Media;

namespace ClickyWindows.Core;

/// <summary>
/// Central design system — mirrors the macOS DS enum.
/// All colors, fonts, and corner radii used across the app come from here.
/// </summary>
public static class DesignSystem
{
    public static class Colors
    {
        // ── Backgrounds ─────────────────────────────────────────────
        public static readonly System.Windows.Media.Color Background = FromHex("#101211");
        public static readonly System.Windows.Media.Color Surface1 = FromHex("#171918");
        public static readonly System.Windows.Media.Color Surface2 = FromHex("#202221");
        public static readonly System.Windows.Media.Color Surface3 = FromHex("#272A29");
        public static readonly System.Windows.Media.Color Surface4 = FromHex("#2E3130");

        // ── Borders ─────────────────────────────────────────────────
        public static readonly System.Windows.Media.Color BorderSubtle = FromHex("#373B39");
        public static readonly System.Windows.Media.Color BorderStrong = FromHex("#444947");

        // ── Text ────────────────────────────────────────────────────
        public static readonly System.Windows.Media.Color TextPrimary = FromHex("#ECEEED");
        public static readonly System.Windows.Media.Color TextSecondary = FromHex("#ADB5B2");
        public static readonly System.Windows.Media.Color TextTertiary = FromHex("#6B736F");

        // ── Tailwind Blue Scale ─────────────────────────────────────
        public static readonly System.Windows.Media.Color Blue50 = FromHex("#eff6ff");
        public static readonly System.Windows.Media.Color Blue100 = FromHex("#dbeafe");
        public static readonly System.Windows.Media.Color Blue200 = FromHex("#bfdbfe");
        public static readonly System.Windows.Media.Color Blue300 = FromHex("#93c5fd");
        public static readonly System.Windows.Media.Color Blue400 = FromHex("#60a5fa");
        public static readonly System.Windows.Media.Color Blue500 = FromHex("#3b82f6");
        public static readonly System.Windows.Media.Color Blue600 = FromHex("#2563eb");
        public static readonly System.Windows.Media.Color Blue700 = FromHex("#1d4ed8");
        public static readonly System.Windows.Media.Color Blue800 = FromHex("#1e40af");
        public static readonly System.Windows.Media.Color Blue900 = FromHex("#1e3a8a");

        // ── Overlay Cursor Colors ───────────────────────────────────
        public static readonly System.Windows.Media.Color OverlayCursorBlue = FromHex("#60A5FA");
        public static readonly System.Windows.Media.Color OverlayCursorBlueHalf = System.Windows.Media.Color.FromArgb(128, 96, 165, 250);
        public static readonly System.Windows.Media.Color OverlayCursorGlow = FromHex("#AEE3FF");

        // ── Semantic ────────────────────────────────────────────────
        public static readonly System.Windows.Media.Color Success = FromHex("#22c55e");
        public static readonly System.Windows.Media.Color Error = FromHex("#ef4444");
        public static readonly System.Windows.Media.Color Warning = FromHex("#f59e0b");

        private static System.Windows.Media.Color FromHex(string hex)
        {
            hex = hex.TrimStart('#');
            byte r = Convert.ToByte(hex[..2], 16);
            byte g = Convert.ToByte(hex[2..4], 16);
            byte b = Convert.ToByte(hex[4..6], 16);
            return System.Windows.Media.Color.FromRgb(r, g, b);
        }

        public static SolidColorBrush Brush(System.Windows.Media.Color c) => new(c);
    }

    public static class CornerRadius
    {
        public const double Small = 4;
        public const double Medium = 8;
        public const double Large = 12;
    }

    public static class FontSizes
    {
        public const double T10 = 10;
        public const double T11 = 11;
        public const double T12 = 12;
        public const double T13 = 13;
        public const double T14 = 14;
        public const double T16 = 16;
    }

    public static class CursorTriangle
    {
        public const double Width = 28;
        public const double Height = 24.25;
        public const double OffsetX = 35;
        public const double OffsetY = 25;
        public const double RotationAngle = 35;
    }
}
