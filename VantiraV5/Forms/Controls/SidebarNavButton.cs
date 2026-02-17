using System.Drawing.Drawing2D;
using VantiraV5.Utils;

namespace VantiraV5.Forms.Controls;

internal sealed class SidebarNavButton : Button
{
    private bool _active;
    private bool _hover;

    public bool Active
    {
        get => _active;
        set
        {
            _active = value;
            Invalidate();
        }
    }

    public SidebarNavButton()
    {
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
        Height = 48;
        Width = 210;
        Margin = new Padding(4, 5, 4, 5);
        TextAlign = ContentAlignment.MiddleLeft;
        ForeColor = ThemePalette.Text;
        Font = new Font("Segoe UI", 10.5F, FontStyle.Bold, GraphicsUnit.Point);
        Cursor = Cursors.Hand;
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);
        _hover = true;
        Invalidate();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        _hover = false;
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        Rectangle bounds = new(0, 0, Width - 1, Height - 1);
        Color background = _active
            ? ThemePalette.SurfaceAlt
            : (_hover ? Color.FromArgb(38, 42, 53) : ThemePalette.Surface);

        using GraphicsPath path = ThemePalette.CreateRoundedPath(bounds, 12);
        using SolidBrush brush = new(background);
        e.Graphics.FillPath(brush, path);

        if (_active)
        {
            using SolidBrush accent = new(ThemePalette.Accent);
            e.Graphics.FillRectangle(accent, 0, 8, 4, Height - 16);
        }

        TextRenderer.DrawText(
            e.Graphics,
            Text,
            Font,
            new Rectangle(16, 0, Width - 20, Height),
            ForeColor,
            TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
    }
}
