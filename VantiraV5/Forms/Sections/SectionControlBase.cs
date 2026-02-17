using VantiraV5.Utils;

namespace VantiraV5.Forms.Sections;

internal abstract class SectionControlBase : UserControl, ISectionView
{
    private readonly ToolTip _toolTip = new();

    public event Action<string>? ChaosRequested;

    public abstract string SectionKey { get; }

    protected SectionControlBase()
    {
        Dock = DockStyle.Fill;
        BackColor = ThemePalette.Background;
        ForeColor = ThemePalette.Text;
        DoubleBuffered = true;
    }

    /// <summary>
    /// Called when the section becomes active in the main shell.
    /// </summary>
    public virtual Task OnActivatedAsync() => Task.CompletedTask;

    protected void RegisterTooltip(Control control, string text) => _toolTip.SetToolTip(control, text);

    protected void TriggerChaos(string source) => ChaosRequested?.Invoke(source);

    protected Label MakeHeader(string text)
    {
        return new Label
        {
            Text = text,
            AutoSize = true,
            Font = new Font("Segoe UI", 19F, FontStyle.Bold, GraphicsUnit.Point),
            ForeColor = ThemePalette.Text,
            Location = new Point(18, 14)
        };
    }
}
