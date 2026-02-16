using VantiraV5.Forms.Controls;
using VantiraV5.Forms.Sections;
using VantiraV5.Services;
using VantiraV5.Utils;

namespace VantiraV5.Forms;

internal sealed class MainForm : Form
{
    private readonly SystemOptimizationService _service = new();
    private readonly Dictionary<string, SectionControlBase> _sections = [];
    private readonly List<SidebarNavButton> _navButtons = [];
    private readonly Panel _contentHost = new();
    private readonly Label _status = new();
    private readonly Timer _petTimer = new();
    private readonly PictureBox _pet = new();

    private int _petDx = 6;
    private int _petDy = 5;

    public MainForm()
    {
        Text = "Vantira V5";
        StartPosition = FormStartPosition.CenterScreen;
        Size = new Size(1280, 800);
        MinimumSize = new Size(1120, 700);
        BackColor = ThemePalette.Background;
        ForeColor = ThemePalette.Text;
        Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
        DoubleBuffered = true;

        BuildLayout();
        BuildSections();
        BuildDesktopPet();

        Resize += (_, _) => UiEffects.ApplyRoundedRegion(this, 18);
        Shown += async (_, _) =>
        {
            UiEffects.ApplyRoundedRegion(this, 18);
            await ActivateSectionAsync("Performance");
        };
    }

    /// <summary>
    /// Constructs app shell including sidebar navigation and content host.
    /// </summary>
    private void BuildLayout()
    {
        Panel sidebar = new()
        {
            Dock = DockStyle.Left,
            Width = 240,
            BackColor = ThemePalette.Surface,
            Padding = new Padding(14)
        };

        Label title = new()
        {
            Text = "Vantira V5",
            Dock = DockStyle.Top,
            Height = 52,
            Font = new Font("Segoe UI", 16F, FontStyle.Bold, GraphicsUnit.Point),
            ForeColor = ThemePalette.Text,
            TextAlign = ContentAlignment.MiddleLeft
        };

        FlowLayoutPanel navFlow = new()
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Padding = new Padding(0, 8, 0, 0)
        };

        _status.Text = "Ready";
        _status.Dock = DockStyle.Bottom;
        _status.Height = 38;
        _status.ForeColor = ThemePalette.MutedText;

        foreach (string section in new[] { "Performance", "Gaming", "Network", "Cleanup", "About" })
        {
            SidebarNavButton nav = new() { Text = section, Tag = section };
            nav.Click += async (_, _) => await ActivateSectionAsync(section);
            navFlow.Controls.Add(nav);
            _navButtons.Add(nav);
        }

        sidebar.Controls.Add(navFlow);
        sidebar.Controls.Add(_status);
        sidebar.Controls.Add(title);

        _contentHost.Dock = DockStyle.Fill;
        _contentHost.BackColor = ThemePalette.Background;
        _contentHost.Padding = new Padding(16);

        Controls.Add(_contentHost);
        Controls.Add(sidebar);
    }

    /// <summary>
    /// Creates each section control and wires Easter-egg event hooks.
    /// </summary>
    private void BuildSections()
    {
        SectionControlBase[] controls =
        [
            new PerformanceSectionControl(_service),
            new GamingSectionControl(_service),
            new NetworkSectionControl(_service),
            new CleanupSectionControl(_service),
            new AboutSectionControl()
        ];

        foreach (SectionControlBase section in controls)
        {
            section.Visible = false;
            section.ChaosRequested += HandleChaosRequest;
            _sections[section.SectionKey] = section;
            _contentHost.Controls.Add(section);
        }
    }

    /// <summary>
    /// Creates a mini desktop pet and timer that bounces around the app window.
    /// </summary>
    private void BuildDesktopPet()
    {
        Bitmap sprite = new(24, 24);
        using (Graphics g = Graphics.FromImage(sprite))
        {
            g.Clear(Color.Transparent);
            g.FillEllipse(new SolidBrush(ThemePalette.Warning), 0, 0, 23, 23);
            g.FillEllipse(Brushes.Black, 6, 8, 3, 3);
            g.FillEllipse(Brushes.Black, 14, 8, 3, 3);
            g.DrawArc(new Pen(Color.Black, 2), 7, 10, 10, 8, 10, 160);
        }

        _pet.Image = sprite;
        _pet.Size = new Size(24, 24);
        _pet.Visible = false;
        _pet.BackColor = Color.Transparent;
        _contentHost.Controls.Add(_pet);

        _petTimer.Interval = 30;
        _petTimer.Tick += (_, _) => MovePet();
    }

    /// <summary>
    /// Switches active section view with lightweight transition and activation callback.
    /// </summary>
    private async Task ActivateSectionAsync(string key)
    {
        if (!_sections.TryGetValue(key, out SectionControlBase? target))
        {
            return;
        }

        foreach ((string name, SectionControlBase section) in _sections)
        {
            section.Visible = name == key;
        }

        foreach (SidebarNavButton nav in _navButtons)
        {
            nav.Active = string.Equals(nav.Tag as string, key, StringComparison.Ordinal);
        }

        _status.Text = $"Loading {key}...";
        await UiEffects.FadeInAsync(target);
        await target.OnActivatedAsync();
        _status.Text = $"Viewing {key}";
    }

    /// <summary>
    /// Triggers fun chaotic Easter eggs after repeated user actions.
    /// </summary>
    private void HandleChaosRequest(string source)
    {
        _status.Text = $"Chaos event: {source}";

        TriggerNotification();
        TriggerInstantOptimizationSimulation();

        if (!_pet.Visible)
        {
            _pet.Visible = true;
            _pet.Location = new Point(30, 30);
            _petTimer.Start();
        }
    }

    private void TriggerNotification()
    {
        Form popup = new()
        {
            FormBorderStyle = FormBorderStyle.None,
            StartPosition = FormStartPosition.Manual,
            Size = new Size(320, 80),
            BackColor = Color.FromArgb(47, 51, 63),
            TopMost = true,
            ShowInTaskbar = false
        };

        Point p = PointToScreen(new Point(Width - 380, 70));
        popup.Location = p;

        Label label = new()
        {
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = Color.FromArgb(122, 243, 110),
            Font = new Font("Consolas", 10F, FontStyle.Bold, GraphicsUnit.Point),
            Text = "[MINECRAFT WARNING] Creeper optimized your FPS!"
        };

        popup.Controls.Add(label);
        popup.Show(this);

        _ = Task.Run(async () =>
        {
            await Task.Delay(1800);
            if (!popup.IsDisposed)
            {
                popup.Invoke(() => popup.Close());
            }
        });
    }

    private void TriggerInstantOptimizationSimulation()
    {
        BackColor = ThemePalette.Surface;
        _contentHost.BackColor = ThemePalette.Surface;

        _ = Task.Run(async () =>
        {
            await Task.Delay(300);
            if (!IsDisposed)
            {
                Invoke(() =>
                {
                    BackColor = ThemePalette.Background;
                    _contentHost.BackColor = ThemePalette.Background;
                });
            }
        });
    }

    private void MovePet()
    {
        if (!_pet.Visible)
        {
            return;
        }

        Rectangle bounds = _contentHost.ClientRectangle;
        int x = _pet.Left + _petDx;
        int y = _pet.Top + _petDy;

        if (x < 0 || x > bounds.Width - _pet.Width)
        {
            _petDx *= -1;
            x = Math.Clamp(x, 0, Math.Max(0, bounds.Width - _pet.Width));
        }

        if (y < 0 || y > bounds.Height - _pet.Height)
        {
            _petDy *= -1;
            y = Math.Clamp(y, 0, Math.Max(0, bounds.Height - _pet.Height));
        }

        _pet.Location = new Point(x, y);
    }
}
