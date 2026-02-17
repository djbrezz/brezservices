using VantiraV5.Utils;

namespace VantiraV5.Forms.Sections;

internal sealed class AboutSectionControl : SectionControlBase
{
    public override string SectionKey => "About";

    public AboutSectionControl()
    {
        Controls.Add(MakeHeader("About"));

        Label text = new()
        {
            Text = "Vantira V5\n\nA modern Windows optimizer built with .NET 8 WinForms.\n\nModules:\n• Performance diagnostics + startup control\n• Network diagnostics + repair commands\n• Cleanup for temp, logs, and recycle bin\n• Gaming mode + FPS booster simulation\n\nEaster egg: click select action buttons 3+ times to trigger chaos mode.",
            Location = new Point(22, 78),
            AutoSize = true,
            MaximumSize = new Size(920, 0),
            ForeColor = ThemePalette.MutedText,
            Font = new Font("Segoe UI", 10.5F, FontStyle.Regular, GraphicsUnit.Point)
        };

        Controls.Add(text);
    }
}
