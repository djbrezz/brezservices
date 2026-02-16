using VantiraV5.Forms.Controls;
using VantiraV5.Models;
using VantiraV5.Services;
using VantiraV5.Utils;

namespace VantiraV5.Forms.Sections;

internal sealed class PerformanceSectionControl : SectionControlBase
{
    private readonly SystemOptimizationService _service;
    private readonly Label _statsLabel = new();
    private readonly ProgressBar _cpuBar = new();
    private readonly ProgressBar _ramBar = new();
    private readonly ListView _startupList = new();
    private int _optimizeTapCount;

    public override string SectionKey => "Performance";

    public PerformanceSectionControl(SystemOptimizationService service)
    {
        _service = service;
        Controls.Add(MakeHeader("Performance"));

        MaterialButton refresh = new() { Text = "Refresh CPU/RAM", Location = new Point(22, 70) };
        refresh.Click += async (_, _) => await RefreshStatsAsync();
        RegisterTooltip(refresh, "Reads current CPU and RAM usage.");

        MaterialButton optimizeMemory = new() { Text = "Optimize Memory", Location = new Point(286, 70) };
        optimizeMemory.Click += async (_, _) =>
        {
            _optimizeTapCount++;
            string result = await _service.OptimizeMemoryAsync();
            MessageBox.Show(result, "Performance", MessageBoxButtons.OK, MessageBoxIcon.Information);

            if (_optimizeTapCount >= 3)
            {
                TriggerChaos("performance-optimize");
                _optimizeTapCount = 0;
            }
        };

        MaterialButton clearTemp = new() { Text = "Quick Temp Cleanup", Location = new Point(550, 70) };
        clearTemp.Click += async (_, _) =>
        {
            CleanupResult cleanup = await _service.ClearTempFilesAsync();
            double mb = cleanup.BytesFreed / 1024d / 1024d;
            MessageBox.Show($"Freed {mb:N2} MB\nFiles: {cleanup.FilesDeleted}\nFolders: {cleanup.DirectoriesDeleted}\nSkipped: {cleanup.Failures}",
                "Quick Cleanup",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        };

        _statsLabel.Location = new Point(22, 126);
        _statsLabel.ForeColor = ThemePalette.Text;
        _statsLabel.AutoSize = true;

        _cpuBar.Location = new Point(22, 158);
        _cpuBar.Width = 380;
        _cpuBar.Style = ProgressBarStyle.Continuous;

        _ramBar.Location = new Point(410, 158);
        _ramBar.Width = 380;
        _ramBar.Style = ProgressBarStyle.Continuous;

        Label startupHeader = new()
        {
            Text = "Startup Applications",
            Location = new Point(22, 205),
            ForeColor = ThemePalette.Text,
            AutoSize = true,
            Font = new Font("Segoe UI", 11F, FontStyle.Bold, GraphicsUnit.Point)
        };

        _startupList.Location = new Point(22, 236);
        _startupList.Size = new Size(900, 300);
        _startupList.BorderStyle = BorderStyle.None;
        _startupList.BackColor = ThemePalette.Surface;
        _startupList.ForeColor = ThemePalette.Text;
        _startupList.FullRowSelect = true;
        _startupList.View = View.Details;
        _startupList.Columns.Add("Name", 180);
        _startupList.Columns.Add("Source", 180);
        _startupList.Columns.Add("Location", 250);
        _startupList.Columns.Add("Command", 280);

        MaterialButton disable = new() { Text = "Disable Selected Startup App", Location = new Point(22, 548), Width = 300 };
        disable.Click += async (_, _) => await DisableSelectedStartupAsync();

        Controls.Add(refresh);
        Controls.Add(optimizeMemory);
        Controls.Add(clearTemp);
        Controls.Add(_statsLabel);
        Controls.Add(_cpuBar);
        Controls.Add(_ramBar);
        Controls.Add(startupHeader);
        Controls.Add(_startupList);
        Controls.Add(disable);
    }

    public override async Task OnActivatedAsync()
    {
        await RefreshStatsAsync();
        await RefreshStartupAppsAsync();
    }

    /// <summary>
    /// Loads and paints CPU/RAM performance bars and textual metrics.
    /// </summary>
    private async Task RefreshStatsAsync()
    {
        SystemSnapshot snapshot = await _service.GetSystemSnapshotAsync();
        _statsLabel.Text = $"CPU: {snapshot.CpuUsagePercent:N1}%   |   RAM: {snapshot.RamUsedGb:N2} / {snapshot.RamTotalGb:N2} GB ({snapshot.RamUsagePercent:N1}%)";
        _cpuBar.Value = Math.Clamp((int)Math.Round(snapshot.CpuUsagePercent), 0, 100);
        _ramBar.Value = Math.Clamp((int)Math.Round(snapshot.RamUsagePercent), 0, 100);
    }

    /// <summary>
    /// Refreshes startup app inventory from service layer.
    /// </summary>
    private async Task RefreshStartupAppsAsync()
    {
        IReadOnlyList<StartupItem> items = await _service.GetStartupAppsAsync();
        _startupList.BeginUpdate();
        _startupList.Items.Clear();

        foreach (StartupItem item in items)
        {
            ListViewItem row = new(item.Name);
            row.SubItems.Add(item.Source);
            row.SubItems.Add(item.Location);
            row.SubItems.Add(item.Command);
            row.Tag = item;
            _startupList.Items.Add(row);
        }

        _startupList.EndUpdate();
    }

    /// <summary>
    /// Disables currently selected startup entry and reloads list.
    /// </summary>
    private async Task DisableSelectedStartupAsync()
    {
        if (_startupList.SelectedItems.Count == 0)
        {
            MessageBox.Show("Select a startup app first.", "Performance", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (_startupList.SelectedItems[0].Tag is not StartupItem startup)
        {
            return;
        }

        bool success = await _service.DisableStartupAppAsync(startup);
        MessageBox.Show(success ? "Startup app disabled." : "Could not disable startup app.", "Performance");
        await RefreshStartupAppsAsync();
    }
}
