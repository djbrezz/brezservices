using VantiraV5.Forms.Controls;
using VantiraV5.Models;
using VantiraV5.Services;
using VantiraV5.Utils;

namespace VantiraV5.Forms.Sections;

internal sealed class NetworkSectionControl : SectionControlBase
{
    private readonly SystemOptimizationService _service;
    private readonly Label _stats = new();
    private readonly TextBox _output = new();
    private int _tapCount;

    public override string SectionKey => "Network";

    public NetworkSectionControl(SystemOptimizationService service)
    {
        _service = service;
        Controls.Add(MakeHeader("Network"));

        MaterialButton speedButton = new() { Text = "Measure Speed + Ping", Location = new Point(22, 70) };
        speedButton.Click += async (_, _) => await RefreshNetworkStatsAsync();

        MaterialButton optimize = new() { Text = "Run Network Optimization", Location = new Point(286, 70) };
        optimize.Click += async (_, _) =>
        {
            _tapCount++;
            _output.Text = await _service.OptimizeNetworkAsync();
            if (_tapCount >= 3)
            {
                TriggerChaos("network-optimize");
                _tapCount = 0;
            }
        };

        _stats.Location = new Point(22, 126);
        _stats.ForeColor = ThemePalette.Text;
        _stats.AutoSize = true;

        _output.Location = new Point(22, 164);
        _output.Size = new Size(900, 385);
        _output.Multiline = true;
        _output.ScrollBars = ScrollBars.Vertical;
        _output.ReadOnly = true;
        _output.BackColor = ThemePalette.Surface;
        _output.ForeColor = ThemePalette.Text;
        _output.BorderStyle = BorderStyle.None;

        Controls.Add(speedButton);
        Controls.Add(optimize);
        Controls.Add(_stats);
        Controls.Add(_output);
    }

    public override async Task OnActivatedAsync() => await RefreshNetworkStatsAsync();

    /// <summary>
    /// Retrieves current network throughput and ping and paints summary line.
    /// </summary>
    private async Task RefreshNetworkStatsAsync()
    {
        NetworkStats stats = await _service.GetNetworkStatsAsync();
        string pingText = stats.PingMs < 0 ? "N/A" : $"{stats.PingMs} ms";
        _stats.Text = $"Download: {stats.DownloadMbps:N2} Mbps | Upload: {stats.UploadMbps:N2} Mbps | Ping: {pingText}";
    }
}
