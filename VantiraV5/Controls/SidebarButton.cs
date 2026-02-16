using System.Drawing.Drawing2D;
using VantiraV5.UI;

namespace VantiraV5.Controls;

internal class SidebarButton : Button
{
    private bool _isActive;

    public bool IsActive
    {
        get => _isActive;
        set
        {
            _isActive = value;
            Invalidate();
        }
    }

    public SidebarButton()
    {
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
        TextAlign = ContentAlignment.MiddleLeft;
        Height = 46;
        Width = 190;
        Font = new Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point);
        ForeColor = Theme.TextPrimary;
        BackColor = Color.Transparent;
        Margin = new Padding(6);
        Cursor = Cursors.Hand;
    }

    protected override void OnPaint(PaintEventArgs pevent)
    {
        pevent.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        var rect = new Rectangle(0, 0, Width - 1, Height - 1);

        Color fill = _isActive ? Theme.SurfaceAlt : Theme.Surface;
        using var path = Theme.RoundedRect(rect, 12);
        using var brush = new SolidBrush(fill);
        pevent.Graphics.FillPath(brush, path);

        if (_isActive)
        {
            using var accent = new SolidBrush(Theme.Accent);
            pevent.Graphics.FillRectangle(accent, new Rectangle(0, 10, 4, Height - 20));
        }

        TextRenderer.DrawText(
            pevent.Graphics,
            Text,
            Font,
            new Rectangle(18, 0, Width - 20, Height),
            ForeColor,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
    }
}
