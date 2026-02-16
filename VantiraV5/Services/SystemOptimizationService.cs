using System.ComponentModel;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using VantiraV5.Models;

namespace VantiraV5.Services;

internal sealed class SystemOptimizationService
{
    private const string StartupRegistryPath = @"Software\Microsoft\Windows\CurrentVersion\Run";

    /// <summary>
    /// Reads CPU and RAM usage values to build a quick performance snapshot.
    /// </summary>
    public async Task<SystemSnapshot> GetSystemSnapshotAsync()
    {
        double cpu = await GetCpuUsageAsync();
        (double used, double total, double usagePercent) = await GetRamUsageAsync();

        return new SystemSnapshot
        {
            CpuUsagePercent = cpu,
            RamUsedGb = used,
            RamTotalGb = total,
            RamUsagePercent = usagePercent
        };
    }

    /// <summary>
    /// Gets current RAM usage using GlobalMemoryStatusEx for stable native Windows metrics.
    /// </summary>
    public async Task<(double UsedGb, double TotalGb, double UsagePercent)> GetRamUsageAsync()
    {
        return await Task.Run(() =>
        {
            MEMORYSTATUSEX state = new() { dwLength = (uint)Marshal.SizeOf<MEMORYSTATUSEX>() };
            if (!GlobalMemoryStatusEx(ref state))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            double total = state.ullTotalPhys / 1024d / 1024d / 1024d;
            double available = state.ullAvailPhys / 1024d / 1024d / 1024d;
            double used = total - available;
            return (used, total, total <= 0 ? 0 : used / total * 100);
        });
    }

    /// <summary>
    /// Measures CPU usage using a short PerformanceCounter sample interval.
    /// </summary>
    public async Task<double> GetCpuUsageAsync()
    {
        return await Task.Run(async () =>
        {
            using PerformanceCounter counter = new("Processor", "% Processor Time", "_Total");
            _ = counter.NextValue();
            await Task.Delay(600);
            return Math.Round(counter.NextValue(), 1);
        });
    }

    /// <summary>
    /// Performs harmless memory optimization simulation to mimic optimizer behavior.
    /// </summary>
    public async Task<string> OptimizeMemoryAsync()
    {
        return await Task.Run(() =>
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            return "Memory compaction simulation complete. GC cycle executed successfully.";
        });
    }

    /// <summary>
    /// Cleans common temporary directories and reports detailed metrics.
    /// </summary>
    public async Task<CleanupResult> ClearTempFilesAsync()
    {
        return await Task.Run(() =>
        {
            string windowsDir = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
            string[] tempPaths =
            [
                Path.GetTempPath(),
                string.IsNullOrWhiteSpace(windowsDir) ? string.Empty : Path.Combine(windowsDir, "Temp")
            ];

            long freed = 0;
            int filesDeleted = 0;
            int directoriesDeleted = 0;
            int failures = 0;

            foreach (string tempPath in tempPaths.Where(p => !string.IsNullOrWhiteSpace(p)).Distinct(StringComparer.OrdinalIgnoreCase))
            {
                if (!Directory.Exists(tempPath))
                {
                    continue;
                }

                try
                {
                    foreach (string file in Directory.EnumerateFiles(tempPath, "*", SearchOption.AllDirectories))
                    {
                        try
                        {
                            FileInfo info = new(file);
                            freed += info.Length;
                            info.Attributes = FileAttributes.Normal;
                            info.Delete();
                            filesDeleted++;
                        }
                        catch
                        {
                            failures++;
                        }
                    }

                    foreach (string folder in Directory.EnumerateDirectories(tempPath, "*", SearchOption.AllDirectories)
                                 .OrderByDescending(d => d.Length))
                    {
                        try
                        {
                            Directory.Delete(folder, false);
                            directoriesDeleted++;
                        }
                        catch
                        {
                            failures++;
                        }
                    }
                }
                catch
                {
                    failures++;
                }
            }

            return new CleanupResult
            {
                BytesFreed = freed,
                FilesDeleted = filesDeleted,
                DirectoriesDeleted = directoriesDeleted,
                Failures = failures
            };
        });
    }

    /// <summary>
    /// Deletes common log-like temporary files while preserving system integrity.
    /// </summary>
    public async Task<int> ClearCommonLogsAsync()
    {
        return await Task.Run(() =>
        {
            int removed = 0;
            string tempPath = Path.GetTempPath();
            if (!Directory.Exists(tempPath))
            {
                return removed;
            }

            foreach (string pattern in new[] { "*.log", "*.etl", "*.tmp" })
            {
                IEnumerable<string> candidates;
                try
                {
                    candidates = Directory.EnumerateFiles(tempPath, pattern, SearchOption.AllDirectories);
                }
                catch
                {
                    continue;
                }

                foreach (string file in candidates)
                {
                    try
                    {
                        File.Delete(file);
                        removed++;
                    }
                    catch
                    {
                        // Non-fatal.
                    }
                }
            }

            return removed;
        });
    }

    /// <summary>
    /// Empties recycle bin using Windows shell API.
    /// </summary>
    public async Task<bool> ClearRecycleBinAsync()
    {
        return await Task.Run(() => SHEmptyRecycleBin(IntPtr.Zero, null,
            RecycleFlags.SHERB_NOCONFIRMATION | RecycleFlags.SHERB_NOPROGRESSUI | RecycleFlags.SHERB_NOSOUND) == 0);
    }

    /// <summary>
    /// Discovers startup applications from registry and startup folders.
    /// </summary>
    public async Task<IReadOnlyList<StartupItem>> GetStartupAppsAsync()
    {
        return await Task.Run(() =>
        {
            List<StartupItem> items = [];
            CollectRegistryStartupItems(Registry.CurrentUser, "HKCU", items);
            CollectRegistryStartupItems(Registry.LocalMachine, "HKLM", items);
            CollectStartupFolderItems(Environment.GetFolderPath(Environment.SpecialFolder.Startup), "Startup Folder (User)", items);
            CollectStartupFolderItems(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartup), "Startup Folder (All Users)", items);

            return (IReadOnlyList<StartupItem>)items
                .OrderBy(i => i.Name, StringComparer.OrdinalIgnoreCase)
                .ThenBy(i => i.Source, StringComparer.OrdinalIgnoreCase)
                .ToList();
        });
    }

    /// <summary>
    /// Disables a startup app by removing registry value or moving shortcut to disabled folder.
    /// </summary>
    public async Task<bool> DisableStartupAppAsync(StartupItem item)
    {
        return await Task.Run(() =>
        {
            if (item.Source.StartsWith("Registry", StringComparison.OrdinalIgnoreCase))
            {
                RegistryKey root = item.Location.StartsWith("HKLM", StringComparison.OrdinalIgnoreCase)
                    ? Registry.LocalMachine
                    : Registry.CurrentUser;

                using RegistryKey? key = root.OpenSubKey(StartupRegistryPath, writable: true);
                if (key is null)
                {
                    return false;
                }

                key.DeleteValue(item.Name, throwOnMissingValue: false);
                return true;
            }

            if (item.Source.StartsWith("Startup Folder", StringComparison.OrdinalIgnoreCase) && File.Exists(item.Command))
            {
                string disabledFolder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "VantiraV5",
                    "DisabledStartup");
                Directory.CreateDirectory(disabledFolder);
                File.Move(item.Command, Path.Combine(disabledFolder, Path.GetFileName(item.Command)), overwrite: true);
                return true;
            }

            return false;
        });
    }

    /// <summary>
    /// Executes network tuning commands and returns command logs.
    /// </summary>
    public async Task<string> OptimizeNetworkAsync()
    {
        string[] commands =
        [
            "ipconfig /flushdns",
            "netsh winsock reset",
            "netsh int ip reset"
        ];

        List<string> logs = [];
        foreach (string command in commands)
        {
            logs.Add(await RunShellCommandAsync(command));
        }

        return string.Join(Environment.NewLine + Environment.NewLine, logs);
    }

    /// <summary>
    /// Samples network throughput and latency (ping) for live diagnostics.
    /// </summary>
    public async Task<NetworkStats> GetNetworkStatsAsync()
    {
        NetworkInterface? active = NetworkInterface.GetAllNetworkInterfaces()
            .Where(n => n.OperationalStatus == OperationalStatus.Up && n.NetworkInterfaceType != NetworkInterfaceType.Loopback)
            .OrderByDescending(n => n.Speed)
            .FirstOrDefault();

        if (active is null)
        {
            return new NetworkStats { DownloadMbps = 0, UploadMbps = 0, PingMs = -1 };
        }

        IPv4InterfaceStatistics start = active.GetIPv4Statistics();
        await Task.Delay(1000);
        IPv4InterfaceStatistics end = active.GetIPv4Statistics();

        double downBps = Math.Max(0, end.BytesReceived - start.BytesReceived);
        double upBps = Math.Max(0, end.BytesSent - start.BytesSent);

        long ping = -1;
        try
        {
            using Ping pinger = new();
            PingReply reply = await pinger.SendPingAsync("8.8.8.8", 1500);
            if (reply.Status == IPStatus.Success)
            {
                ping = reply.RoundtripTime;
            }
        }
        catch
        {
            ping = -1;
        }

        return new NetworkStats
        {
            DownloadMbps = Math.Round(downBps * 8 / 1_000_000, 2),
            UploadMbps = Math.Round(upBps * 8 / 1_000_000, 2),
            PingMs = ping
        };
    }

    /// <summary>
    /// Enables gaming power profile (ultimate if available, else high performance).
    /// </summary>
    public async Task<string> EnableGamingModeAsync()
    {
        string list = await RunShellCommandAsync("powercfg /list");
        string scheme = list.Contains("Ultimate Performance", StringComparison.OrdinalIgnoreCase)
            ? "e9a42b02-d5df-448d-aa00-03f14749eb61"
            : "SCHEME_MIN";

        return await RunShellCommandAsync($"powercfg /setactive {scheme}");
    }

    /// <summary>
    /// Simulates an FPS booster with harmless process-priority style behavior.
    /// </summary>
    public async Task<string> SimulateFpsBoostAsync()
    {
        return await Task.Run(() =>
        {
            Thread.Sleep(400);
            return "FPS Booster Simulation: Background scheduler tuned, visual latency profile optimized (simulated).";
        });
    }

    private static void CollectRegistryStartupItems(RegistryKey root, string rootName, ICollection<StartupItem> items)
    {
        using RegistryKey? key = root.OpenSubKey(StartupRegistryPath, writable: false);
        if (key is null)
        {
            return;
        }

        foreach (string name in key.GetValueNames())
        {
            items.Add(new StartupItem
            {
                Name = name,
                Source = "Registry",
                Location = $"{rootName}\\{StartupRegistryPath}",
                Command = key.GetValue(name)?.ToString() ?? string.Empty
            });
        }
    }

    private static void CollectStartupFolderItems(string startupFolder, string source, ICollection<StartupItem> items)
    {
        if (!Directory.Exists(startupFolder))
        {
            return;
        }

        foreach (string file in Directory.EnumerateFiles(startupFolder))
        {
            items.Add(new StartupItem
            {
                Name = Path.GetFileNameWithoutExtension(file),
                Source = source,
                Location = startupFolder,
                Command = file
            });
        }
    }

    private static async Task<string> RunShellCommandAsync(string command)
    {
        using Process process = new();
        process.StartInfo = new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = $"/c {command}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        process.Start();
        string output = await process.StandardOutput.ReadToEndAsync();
        string error = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        string text = $"> {command}{Environment.NewLine}{output.Trim()}";
        if (!string.IsNullOrWhiteSpace(error))
        {
            text += $"{Environment.NewLine}{error.Trim()}";
        }

        return $"{text}{Environment.NewLine}Exit code: {process.ExitCode}";
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GlobalMemoryStatusEx(ref MEMORYSTATUSEX lpBuffer);

    [DllImport("Shell32.dll", CharSet = CharSet.Unicode)]
    private static extern int SHEmptyRecycleBin(IntPtr hwnd, string? pszRootPath, RecycleFlags dwFlags);

    [Flags]
    private enum RecycleFlags : uint
    {
        SHERB_NOCONFIRMATION = 0x00000001,
        SHERB_NOPROGRESSUI = 0x00000002,
        SHERB_NOSOUND = 0x00000004
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private struct MEMORYSTATUSEX
    {
        public uint dwLength;
        public uint dwMemoryLoad;
        public ulong ullTotalPhys;
        public ulong ullAvailPhys;
        public ulong ullTotalPageFile;
        public ulong ullAvailPageFile;
        public ulong ullTotalVirtual;
        public ulong ullAvailVirtual;
        public ulong ullAvailExtendedVirtual;
    }
}
