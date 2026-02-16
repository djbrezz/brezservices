using System.Drawing.Drawing2D;
using VantiraV5.Controls;
using VantiraV5.Models;
using VantiraV5.Services;

namespace VantiraV5.UI;

internal sealed class MainForm : Form
{
    private readonly SystemOptimizationService _optimizer = new();

    private readonly Panel _sidebar = new();
    private readonly Panel _contentHost = new();
    private readonly Label _statusLabel = new();
    private readonly Dictionary<string, Control> _sectionViews = new();
    private readonly List<SidebarButton> _navButtons = [];

    private readonly Label _ramLabel = new();
    private readonly ProgressBar _ramProgress = new();
    private readonly ListView _startupList = new();
    private readonly TextBox _networkOutput = new();
    private readonly Label _cleanupLabel = new();

    public MainForm()
    {
        Text = "Vantira V5";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(1150, 720);
        Size = new Size(1250, 780);
        BackColor = Theme.Background;
        ForeColor = Theme.TextPrimary;
        Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
        DoubleBuffered = true;

        BuildShell();
        BuildSections();

        Resize += (_, _) => ApplyRoundedCorners();
        Shown += async (_, _) =>
        {
            ApplyRoundedCorners();
            await RefreshPerformanceAsync();
            await RefreshStartupAppsAsync();
        };
    }

    private void BuildShell()
    {
        _sidebar.Dock = DockStyle.Left;
        _sidebar.Width = 230;
        _sidebar.BackColor = Theme.Surface;
        _sidebar.Padding = new Padding(16, 18, 16, 18);

        Label title = new()
        {
            Text = "Vantira V5",
            Dock = DockStyle.Top,
            Height = 52,
            Font = new Font("Segoe UI", 16F, FontStyle.Bold, GraphicsUnit.Point),
            ForeColor = Theme.TextPrimary,
            TextAlign = ContentAlignment.MiddleLeft
        };

        _statusLabel.Text = "Ready";
        _statusLabel.Dock = DockStyle.Bottom;
        _statusLabel.Height = 40;
        _statusLabel.ForeColor = Theme.TextMuted;

        FlowLayoutPanel navLayout = new()
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Padding = new Padding(0, 16, 0, 0)
        };

        AddNavButton(navLayout, "Performance");
        AddNavButton(navLayout, "Gaming");
        AddNavButton(navLayout, "Network");
        AddNavButton(navLayout, "Cleanup");
        AddNavButton(navLayout, "About");

        _sidebar.Controls.Add(navLayout);
        _sidebar.Controls.Add(_statusLabel);
        _sidebar.Controls.Add(title);

        _contentHost.Dock = DockStyle.Fill;
        _contentHost.BackColor = Theme.Background;
        _contentHost.Padding = new Padding(20);

        Controls.Add(_contentHost);
        Controls.Add(_sidebar);
    }

    private void AddNavButton(Control parent, string section)
    {
        SidebarButton button = new() { Text = section, Tag = section };
        button.Click += (_, _) => ShowSection(section);
        parent.Controls.Add(button);
        _navButtons.Add(button);
    }

    private void BuildSections()
    {
        _sectionViews["Performance"] = BuildPerformanceView();
        _sectionViews["Gaming"] = BuildGamingView();
        _sectionViews["Network"] = BuildNetworkView();
        _sectionViews["Cleanup"] = BuildCleanupView();
        _sectionViews["About"] = BuildAboutView();

        foreach (Control section in _sectionViews.Values)
        {
            section.Visible = false;
            _contentHost.Controls.Add(section);
        }

        ShowSection("Performance");
    }

    private Control BuildPerformanceView()
    {
        Panel panel = CreateSectionPanel("Performance");

        Button refreshButton = CreateActionButton("Check RAM Usage");
        refreshButton.Location = new Point(24, 80);
        refreshButton.Click += async (_, _) => await RefreshPerformanceAsync();

        _ramLabel.AutoSize = true;
        _ramLabel.Location = new Point(24, 136);
        _ramLabel.ForeColor = Theme.TextPrimary;

        _ramProgress.Location = new Point(24, 170);
        _ramProgress.Width = 560;
        _ramProgress.Height = 24;
        _ramProgress.Style = ProgressBarStyle.Continuous;

        Label startupHeader = new()
        {
            Text = "Startup Apps",
            Location = new Point(24, 230),
            AutoSize = true,
            Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point)
        };

        _startupList.View = View.Details;
        _startupList.FullRowSelect = true;
        _startupList.GridLines = false;
        _startupList.BorderStyle = BorderStyle.None;
        _startupList.BackColor = Theme.Surface;
        _startupList.ForeColor = Theme.TextPrimary;
        _startupList.Location = new Point(24, 266);
        _startupList.Size = new Size(720, 280);
        _startupList.Columns.Add("Name", 220);
        _startupList.Columns.Add("Source", 170);
        _startupList.Columns.Add("Location", 220);
        _startupList.Columns.Add("Command", 260);

        Button disableButton = CreateActionButton("Disable Selected Startup App");
        disableButton.Location = new Point(24, 562);
        disableButton.Click += async (_, _) => await DisableSelectedStartupAsync();

        panel.Controls.Add(refreshButton);
        panel.Controls.Add(_ramLabel);
        panel.Controls.Add(_ramProgress);
        panel.Controls.Add(startupHeader);
        panel.Controls.Add(_startupList);
        panel.Controls.Add(disableButton);

        return panel;
    }

    private Control BuildGamingView()
    {
        Panel panel = CreateSectionPanel("Gaming");

        Label description = CreateBodyLabel("Apply a high-performance power profile to prioritize gaming responsiveness.");
        description.Location = new Point(24, 86);

        Button activate = CreateActionButton("Enable Gaming Power Mode");
        activate.Location = new Point(24, 130);
        activate.Click += async (_, _) =>
        {
            await RunActionAsync("Applying gaming profile...", async () =>
            {
                string result = await _optimizer.EnableGamingModeAsync();
                MessageBox.Show(result, "Gaming Mode", MessageBoxButtons.OK, MessageBoxIcon.Information);
            });
        };

        panel.Controls.Add(description);
        panel.Controls.Add(activate);
        return panel;
    }

    private Control BuildNetworkView()
    {
        Panel panel = CreateSectionPanel("Network");

        Label description = CreateBodyLabel("Refresh network stack by flushing DNS cache and resetting Winsock.");
        description.Location = new Point(24, 86);

        Button optimize = CreateActionButton("Optimize Network");
        optimize.Location = new Point(24, 130);
        optimize.Click += async (_, _) =>
        {
            await RunActionAsync("Optimizing network...", async () =>
            {
                _networkOutput.Text = await _optimizer.OptimizeNetworkAsync();
            });
        };

        _networkOutput.Location = new Point(24, 190);
        _networkOutput.Size = new Size(760, 340);
        _networkOutput.Multiline = true;
        _networkOutput.ScrollBars = ScrollBars.Vertical;
        _networkOutput.ReadOnly = true;
        _networkOutput.BorderStyle = BorderStyle.None;
        _networkOutput.BackColor = Theme.Surface;
        _networkOutput.ForeColor = Theme.TextPrimary;

        panel.Controls.Add(description);
        panel.Controls.Add(optimize);
        panel.Controls.Add(_networkOutput);

        return panel;
    }

    private Control BuildCleanupView()
    {
        Panel panel = CreateSectionPanel("Cleanup");

        Label description = CreateBodyLabel("Reclaim storage by deleting unused temporary files in your user temp directory.");
        description.Location = new Point(24, 86);

        Button clean = CreateActionButton("Clear Temp Files");
        clean.Location = new Point(24, 130);
        clean.Click += async (_, _) =>
        {
            await RunActionAsync("Clearing temporary files...", async () =>
            {
                var result = await _optimizer.ClearTempFilesAsync();
                double mb = result.BytesFreed / 1024d / 1024d;
                _cleanupLabel.Text = $"Freed {mb:N2} MB | Files: {result.FilesDeleted} | Folders: {result.DirectoriesDeleted} | Skipped: {result.Failures}";
            });
        };

        _cleanupLabel.Location = new Point(24, 190);
        _cleanupLabel.AutoSize = true;
        _cleanupLabel.ForeColor = Theme.TextPrimary;
        _cleanupLabel.Text = "No cleanup run yet.";

        panel.Controls.Add(description);
        panel.Controls.Add(clean);
        panel.Controls.Add(_cleanupLabel);

        return panel;
    }

    private Control BuildAboutView()
    {
        Panel panel = CreateSectionPanel("About");

        Label about = CreateBodyLabel("Vantira V5 is a lightweight Windows optimizer built on .NET 8 WinForms.\n\n" +
                                      "Included modules:\n" +
                                      "• Performance diagnostics (RAM + startup apps)\n" +
                                      "• Gaming power optimization\n" +
                                      "• Network reset tools\n" +
                                      "• Temp-file cleanup utility\n\n" +
                                      "Always run optimization actions with appropriate privileges.");
        about.Location = new Point(24, 86);
        about.MaximumSize = new Size(780, 0);

        panel.Controls.Add(about);
        return panel;
    }

    private Panel CreateSectionPanel(string title)
    {
        Panel panel = new()
        {
            Dock = DockStyle.Fill,
            BackColor = Theme.Background,
            Padding = new Padding(12)
        };

        Label header = new()
        {
            Text = title,
            Font = new Font("Segoe UI", 20F, FontStyle.Bold, GraphicsUnit.Point),
            ForeColor = Theme.TextPrimary,
            AutoSize = true,
            Location = new Point(24, 24)
        };

        panel.Controls.Add(header);
        return panel;
    }

    private static Label CreateBodyLabel(string text)
    {
        return new Label
        {
            Text = text,
            ForeColor = Theme.TextMuted,
            Font = new Font("Segoe UI", 10.5F, FontStyle.Regular, GraphicsUnit.Point),
            AutoSize = true
        };
    }

    private static Button CreateActionButton(string text)
    {
        Button button = new()
        {
            Text = text,
            Width = 260,
            Height = 42,
            FlatStyle = FlatStyle.Flat,
            BackColor = Theme.Accent,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point),
            Cursor = Cursors.Hand
        };

        button.FlatAppearance.BorderSize = 0;
        return button;
    }

    private void ShowSection(string section)
    {
        foreach ((string key, Control view) in _sectionViews)
        {
            view.Visible = key == section;
        }

        foreach (SidebarButton button in _navButtons)
        {
            button.IsActive = string.Equals(button.Tag as string, section, StringComparison.Ordinal);
        }

        _statusLabel.Text = $"Viewing {section}";

        _ = section switch
        {
            "Performance" => WarmPerformanceSectionAsync(),
            _ => Task.CompletedTask
        };
    }


    private async Task WarmPerformanceSectionAsync()
    {
        if (string.IsNullOrWhiteSpace(_ramLabel.Text))
        {
            await RefreshPerformanceAsync();
        }

        if (_startupList.Items.Count == 0)
        {
            await RefreshStartupAppsAsync();
        }
    }
    private async Task RefreshPerformanceAsync()
    {
        await RunActionAsync("Checking RAM usage...", async () =>
        {
            var stats = await _optimizer.GetRamUsageAsync();
            _ramLabel.Text = $"RAM Usage: {stats.UsedGb:N2} GB / {stats.TotalGb:N2} GB ({stats.UsagePercent:N1}%)";
            _ramProgress.Value = Math.Clamp((int)Math.Round(stats.UsagePercent), 0, 100);
        });
    }

    private async Task RefreshStartupAppsAsync()
    {
        IReadOnlyList<StartupItem> apps = await _optimizer.GetStartupAppsAsync();

        _startupList.BeginUpdate();
        _startupList.Items.Clear();

        foreach (StartupItem app in apps)
        {
            ListViewItem item = new(app.Name);
            item.SubItems.Add(app.Source);
            item.SubItems.Add(app.Location);
            item.SubItems.Add(app.Command);
            item.Tag = app;
            _startupList.Items.Add(item);
        }

        _startupList.EndUpdate();
    }

    private async Task DisableSelectedStartupAsync()
    {
        if (_startupList.SelectedItems.Count == 0)
        {
            MessageBox.Show("Select a startup app first.", "No selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (_startupList.SelectedItems[0].Tag is not StartupItem selected)
        {
            return;
        }

        await RunActionAsync($"Disabling {selected.Name}...", async () =>
        {
            bool success = await _optimizer.DisableStartupAppAsync(selected);
            if (!success)
            {
                MessageBox.Show("Unable to disable selected startup app.", "Startup", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            await RefreshStartupAppsAsync();
        });
    }

    private async Task RunActionAsync(string status, Func<Task> operation)
    {
        _statusLabel.Text = status;
        try
        {
            await operation();
            _statusLabel.Text = "Done";
        }
        catch (Exception ex)
        {
            _statusLabel.Text = "Action failed";
            MessageBox.Show(ex.Message, "Vantira V5", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ApplyRoundedCorners()
    {
        Rectangle rect = new(0, 0, Width, Height);
        using GraphicsPath path = Theme.RoundedRect(rect, 18);
        Region = new Region(path);
    }
}
