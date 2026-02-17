using System.Drawing.Drawing2D;

namespace VantiraV5.Utils;

internal static class ThemePalette
{
    public static readonly Color Background = Color.FromArgb(18, 20, 26);
    public static readonly Color Surface = Color.FromArgb(30, 33, 42);
    public static readonly Color SurfaceAlt = Color.FromArgb(43, 48, 61);
    public static readonly Color Accent = Color.FromArgb(87, 132, 255);
    public static readonly Color AccentHover = Color.FromArgb(105, 149, 255);
    public static readonly Color Success = Color.FromArgb(76, 196, 138);
    public static readonly Color Warning = Color.FromArgb(245, 178, 66);
    public static readonly Color Text = Color.FromArgb(236, 239, 244);
    public static readonly Color MutedText = Color.FromArgb(150, 158, 178);

    /// <summary>
    /// Creates a rounded rectangle path used by controls and forms for modern corners.
    /// </summary>
    public static GraphicsPath CreateRoundedPath(Rectangle rectangle, int radius)
    {
        int diameter = radius * 2;
        GraphicsPath path = new();

        path.AddArc(rectangle.X, rectangle.Y, diameter, diameter, 180, 90);
        path.AddArc(rectangle.Right - diameter, rectangle.Y, diameter, diameter, 270, 90);
        path.AddArc(rectangle.Right - diameter, rectangle.Bottom - diameter, diameter, diameter, 0, 90);
        path.AddArc(rectangle.X, rectangle.Bottom - diameter, diameter, diameter, 90, 90);
        path.CloseFigure();

        return path;
    }
}
