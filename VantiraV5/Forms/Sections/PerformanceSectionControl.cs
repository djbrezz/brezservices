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
    private readonly TextBox _insightsBox = new();
    private int _optimizeTapCount;

    public override string SectionKey => "Performance";

    public PerformanceSectionControl(SystemOptimizationService service)
    {
        _service = service;
        Controls.Add(MakeHeader("Performance"));

        MaterialButton refresh = new() { Text = "Refresh CPU / RAM", Location = new Point(22, 70), Width = 220 };
        refresh.Click += async (_, _) => await RefreshStatsAsync();
        RegisterTooltip(refresh, "Fetch live system performance snapshot.");

        MaterialButton optimizeMemory = new() { Text = "Optimize Memory", Location = new Point(252, 70), Width = 220 };
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
        RegisterTooltip(optimizeMemory, "Runs safe memory compaction simulation.");

        MaterialButton aiScan = new() { Text = "Run AI Health Scan", Location = new Point(482, 70), Width = 220 };
        aiScan.Click += async (_, _) => _insightsBox.Text = await _service.BuildHealthSummaryAsync();
        RegisterTooltip(aiScan, "Generates a readable system-health summary.");

        MaterialButton clearTemp = new() { Text = "Quick Temp Cleanup", Location = new Point(712, 70), Width = 220 };
        clearTemp.Click += async (_, _) =>
        {
            CleanupResult cleanup = await _service.ClearTempFilesAsync();
            double mb = cleanup.BytesFreed / 1024d / 1024d;
            _insightsBox.Text = $"Cleanup Result\n• Freed: {mb:N2} MB\n• Files: {cleanup.FilesDeleted}\n• Folders: {cleanup.DirectoriesDeleted}\n• Skipped: {cleanup.Failures}";
        };

        _statsLabel.Location = new Point(22, 126);
        _statsLabel.ForeColor = ThemePalette.Text;
        _statsLabel.AutoSize = true;

        _cpuBar.Location = new Point(22, 154);
        _cpuBar.Width = 450;
        _cpuBar.Style = ProgressBarStyle.Continuous;
        _cpuBar.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

        _ramBar.Location = new Point(482, 154);
        _ramBar.Width = 450;
        _ramBar.Style = ProgressBarStyle.Continuous;
        _ramBar.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

        GroupBox startupGroup = new()
        {
            Text = "Startup Applications",
            ForeColor = ThemePalette.Text,
            BackColor = ThemePalette.Background,
            Location = new Point(22, 196),
            Size = new Size(620, 430),
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left
        };

        _startupList.Dock = DockStyle.Fill;
        _startupList.BorderStyle = BorderStyle.None;
        _startupList.BackColor = ThemePalette.Surface;
        _startupList.ForeColor = ThemePalette.Text;
        _startupList.FullRowSelect = true;
        _startupList.View = View.Details;
        _startupList.Columns.Add("Name", 130);
        _startupList.Columns.Add("Source", 140);
        _startupList.Columns.Add("Location", 160);
        _startupList.Columns.Add("Command", 170);
        startupGroup.Controls.Add(_startupList);

        MaterialButton disable = new()
        {
            Text = "Disable Selected Startup App",
            Width = 320,
            Location = new Point(22, 634),
            Anchor = AnchorStyles.Left | AnchorStyles.Bottom
        };
        disable.Click += async (_, _) => await DisableSelectedStartupAsync();

        GroupBox insightsGroup = new()
        {
            Text = "AI Insights",
            ForeColor = ThemePalette.Text,
            BackColor = ThemePalette.Background,
            Location = new Point(652, 196),
            Size = new Size(280, 468),
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
        };

        _insightsBox.Dock = DockStyle.Fill;
        _insightsBox.Multiline = true;
        _insightsBox.ReadOnly = true;
        _insightsBox.ScrollBars = ScrollBars.Vertical;
        _insightsBox.BackColor = ThemePalette.Surface;
        _insightsBox.ForeColor = ThemePalette.Text;
        _insightsBox.BorderStyle = BorderStyle.None;
        _insightsBox.Text = "Run AI Health Scan to generate optimization insights.";
        insightsGroup.Controls.Add(_insightsBox);

        Controls.Add(refresh);
        Controls.Add(optimizeMemory);
        Controls.Add(aiScan);
        Controls.Add(clearTemp);
        Controls.Add(_statsLabel);
        Controls.Add(_cpuBar);
        Controls.Add(_ramBar);
        Controls.Add(startupGroup);
        Controls.Add(disable);
        Controls.Add(insightsGroup);

        Resize += (_, _) => UpdateResponsiveLayout();
        UpdateResponsiveLayout();
    }

    public override async Task OnActivatedAsync()
    {
        await RefreshStatsAsync();
        await RefreshStartupAppsAsync();
    }

    /// <summary>
    /// Updates control placement to keep layout clean on resize.
    /// </summary>
    private void UpdateResponsiveLayout()
    {
        int panelWidth = ClientSize.Width - 44;
        int rightColumnWidth = Math.Max(260, panelWidth - 630);

        foreach (Control control in Controls)
        {
            if (control is GroupBox group && group.Text == "AI Insights")
            {
                group.Left = 652;
                group.Width = rightColumnWidth;
            }
        }
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
