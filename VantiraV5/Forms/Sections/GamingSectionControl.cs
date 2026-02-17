using VantiraV5.Forms.Controls;
using VantiraV5.Services;
using VantiraV5.Utils;

namespace VantiraV5.Forms.Sections;

internal sealed class GamingSectionControl : SectionControlBase
{
    private readonly SystemOptimizationService _service;
    private readonly Label _status = new();
    private int _tapCount;

    public override string SectionKey => "Gaming";

    public GamingSectionControl(SystemOptimizationService service)
    {
        _service = service;
        Controls.Add(MakeHeader("Gaming"));

        Label desc = new()
        {
            Text = "Gaming mode applies high-performance power settings and optional FPS booster simulation.",
            Location = new Point(22, 76),
            AutoSize = true,
            ForeColor = ThemePalette.MutedText
        };

        MaterialButton mode = new() { Text = "Enable Gaming Power Mode", Location = new Point(22, 116), Width = 300 };
        mode.Click += async (_, _) =>
        {
            _tapCount++;
            _status.Text = await _service.EnableGamingModeAsync();

            if (_tapCount >= 3)
            {
                TriggerChaos("gaming-mode");
                _tapCount = 0;
            }
        };

        MaterialButton fps = new() { Text = "Run FPS Booster (Simulated)", Location = new Point(334, 116), Width = 300 };
        fps.Click += async (_, _) => _status.Text = await _service.SimulateFpsBoostAsync();

        _status.Location = new Point(22, 178);
        _status.AutoSize = true;
        _status.MaximumSize = new Size(900, 0);
        _status.ForeColor = ThemePalette.Text;
        _status.Text = "Waiting for gaming optimization.";

        Controls.Add(desc);
        Controls.Add(mode);
        Controls.Add(fps);
        Controls.Add(_status);
    }
}
