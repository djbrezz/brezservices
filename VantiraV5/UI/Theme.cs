using System.Drawing.Drawing2D;

namespace VantiraV5.UI;

internal static class Theme
{
    public static readonly Color Background = Color.FromArgb(24, 26, 32);
    public static readonly Color Surface = Color.FromArgb(32, 35, 43);
    public static readonly Color SurfaceAlt = Color.FromArgb(43, 47, 58);
    public static readonly Color Accent = Color.FromArgb(0, 170, 255);
    public static readonly Color TextPrimary = Color.FromArgb(236, 240, 244);
    public static readonly Color TextMuted = Color.FromArgb(155, 165, 183);

    public static GraphicsPath RoundedRect(Rectangle bounds, int radius)
    {
        int diameter = radius * 2;
        var path = new GraphicsPath();

        path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);
        path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 90);
        path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
        path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90, 90);
        path.CloseFigure();

        return path;
    }
}
