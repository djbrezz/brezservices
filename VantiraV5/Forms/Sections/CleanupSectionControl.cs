using VantiraV5.Forms.Controls;
using VantiraV5.Models;
using VantiraV5.Services;
using VantiraV5.Utils;

namespace VantiraV5.Forms.Sections;

internal sealed class CleanupSectionControl : SectionControlBase
{
    private readonly SystemOptimizationService _service;
    private readonly Label _status = new();
    private int _tapCount;

    public override string SectionKey => "Cleanup";

    public CleanupSectionControl(SystemOptimizationService service)
    {
        _service = service;
        Controls.Add(MakeHeader("Cleanup"));

        MaterialButton clearTemp = new() { Text = "Clear Temp Files", Location = new Point(22, 70), Width = 210 };
        clearTemp.Click += async (_, _) =>
        {
            _tapCount++;
            CleanupResult result = await _service.ClearTempFilesAsync();
            double mb = result.BytesFreed / 1024d / 1024d;
            _status.Text = $"Temp cleanup freed {mb:N2} MB | Files {result.FilesDeleted} | Folders {result.DirectoriesDeleted} | Skipped {result.Failures}";

            if (_tapCount >= 3)
            {
                TriggerChaos("cleanup-temp");
                _tapCount = 0;
            }
        };

        MaterialButton recycle = new() { Text = "Empty Recycle Bin", Location = new Point(244, 70), Width = 210 };
        recycle.Click += async (_, _) =>
        {
            bool ok = await _service.ClearRecycleBinAsync();
            _status.Text = ok ? "Recycle Bin cleaned successfully." : "Could not clear Recycle Bin (permission or policy).";
        };

        MaterialButton logs = new() { Text = "Clear Temp Logs", Location = new Point(466, 70), Width = 210 };
        logs.Click += async (_, _) =>
        {
            int removed = await _service.ClearCommonLogsAsync();
            _status.Text = $"Removed {removed} trace/log files from temporary locations.";
        };

        _status.Location = new Point(22, 130);
        _status.AutoSize = true;
        _status.ForeColor = ThemePalette.Text;
        _status.MaximumSize = new Size(920, 0);
        _status.Text = "Run a cleanup action to see details.";

        Controls.Add(clearTemp);
        Controls.Add(recycle);
        Controls.Add(logs);
        Controls.Add(_status);
    }
}
