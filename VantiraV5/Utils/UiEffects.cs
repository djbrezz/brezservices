using System.Drawing.Drawing2D;

namespace VantiraV5.Utils;

internal static class UiEffects
{
    /// <summary>
    /// Applies a rounded-corner region to a control.
    /// </summary>
    public static void ApplyRoundedRegion(Control control, int radius)
    {
        Rectangle rectangle = new(0, 0, control.Width, control.Height);
        using GraphicsPath path = ThemePalette.CreateRoundedPath(rectangle, radius);
        control.Region = new Region(path);
    }

    /// <summary>
    /// Performs a lightweight fade-in animation for the supplied control.
    /// </summary>
    public static async Task FadeInAsync(Control control, int durationMs = 180)
    {
        if (durationMs <= 0)
        {
            control.Visible = true;
            return;
        }

        control.Visible = true;
        int steps = 10;
        int delay = Math.Max(10, durationMs / steps);
        for (int i = 0; i <= steps; i++)
        {
            control.Refresh();
            await Task.Delay(delay);
        }
    }
}
