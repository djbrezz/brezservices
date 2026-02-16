using VantiraV5.Utils;

namespace VantiraV5.Forms.Controls;

internal sealed class MaterialButton : Button
{
    private bool _hovering;

    public MaterialButton()
    {
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
        Height = 42;
        Width = 250;
        Cursor = Cursors.Hand;
        BackColor = ThemePalette.Accent;
        ForeColor = Color.White;
        Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold, GraphicsUnit.Point);
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        UiEffects.ApplyRoundedRegion(this, 12);
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);
        _hovering = true;
        Invalidate();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        _hovering = false;
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs pevent)
    {
        BackColor = _hovering ? ThemePalette.AccentHover : ThemePalette.Accent;
        base.OnPaint(pevent);
    }
}
