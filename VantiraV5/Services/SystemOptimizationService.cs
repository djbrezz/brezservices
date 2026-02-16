using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using VantiraV5.Models;

namespace VantiraV5.Services;

internal sealed class SystemOptimizationService
{
    private const string StartupRegistryPath = @"Software\Microsoft\Windows\CurrentVersion\Run";

    public async Task<(double UsedGb, double TotalGb, double UsagePercent)> GetRamUsageAsync()
    {
        return await Task.Run(() =>
        {
            MEMORYSTATUSEX statex = new() { dwLength = (uint)Marshal.SizeOf<MEMORYSTATUSEX>() };
            if (!GlobalMemoryStatusEx(ref statex))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            double total = statex.ullTotalPhys / 1024d / 1024d / 1024d;
            double available = statex.ullAvailPhys / 1024d / 1024d / 1024d;
            double used = total - available;
            double percent = total <= 0 ? 0 : used / total * 100;
            return (used, total, percent);
        });
    }

    public async Task<CleanupResult> ClearTempFilesAsync()
    {
        return await Task.Run(() =>
        {
            string[] tempPaths =
            [
                Path.GetTempPath(),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Temp")
            ];

            long deletedBytes = 0;
            int deletedFiles = 0;
            int deletedDirectories = 0;
            int failures = 0;

            foreach (string tempPath in tempPaths.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                if (!Directory.Exists(tempPath))
                {
                    continue;
                }

                IEnumerable<string> files;
                try
                {
                    files = Directory.EnumerateFiles(tempPath, "*", SearchOption.AllDirectories);
                }
                catch
                {
                    failures++;
                    continue;
                }

                foreach (string file in files)
                {
                    try
                    {
                        FileInfo info = new(file);
                        deletedBytes += info.Length;
                        info.Attributes = FileAttributes.Normal;
                        info.Delete();
                        deletedFiles++;
                    }
                    catch
                    {
                        failures++;
                    }
                }

                IEnumerable<string> directories;
                try
                {
                    directories = Directory.EnumerateDirectories(tempPath, "*", SearchOption.AllDirectories)
                        .OrderByDescending(d => d.Length);
                }
                catch
                {
                    failures++;
                    continue;
                }

                foreach (string dir in directories)
                {
                    try
                    {
                        Directory.Delete(dir, false);
                        deletedDirectories++;
                    }
                    catch
                    {
                        failures++;
                    }
                }
            }

            return new CleanupResult
            {
                BytesFreed = deletedBytes,
                FilesDeleted = deletedFiles,
                DirectoriesDeleted = deletedDirectories,
                Failures = failures
            };
        });
    }

    public async Task<IReadOnlyList<StartupItem>> GetStartupAppsAsync()
    {
        return await Task.Run(() =>
        {
            var items = new List<StartupItem>();

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

    public async Task<bool> DisableStartupAppAsync(StartupItem item)
    {
        return await Task.Run(() =>
        {
            if (item.Source.StartsWith("Registry", StringComparison.OrdinalIgnoreCase))
            {
                RegistryKey? root = item.Location.StartsWith("HKLM", StringComparison.OrdinalIgnoreCase)
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
                string destination = Path.Combine(disabledFolder, Path.GetFileName(item.Command));
                File.Move(item.Command, destination, overwrite: true);
                return true;
            }

            return false;
        });
    }

    public async Task<string> OptimizeNetworkAsync()
    {
        string[] commands =
        [
            "ipconfig /flushdns",
            "netsh winsock reset",
            "netsh int ip reset"
        ];

        var logs = new List<string>();
        foreach (string command in commands)
        {
            logs.Add(await RunShellCommandAsync(command));
        }

        return string.Join(Environment.NewLine + Environment.NewLine, logs);
    }

    public async Task<string> EnableGamingModeAsync()
    {
        string query = await RunShellCommandAsync("powercfg /list");
        string preferredScheme = query.Contains("Ultimate Performance", StringComparison.OrdinalIgnoreCase)
            ? "e9a42b02-d5df-448d-aa00-03f14749eb61"
            : "SCHEME_MIN";

        return await RunShellCommandAsync($"powercfg /setactive {preferredScheme}");
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
            string command = key.GetValue(name)?.ToString() ?? string.Empty;
            items.Add(new StartupItem
            {
                Name = name,
                Source = "Registry",
                Location = $"{rootName}\\{StartupRegistryPath}",
                Command = command
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

        string log = $"> {command}{Environment.NewLine}{output.Trim()}";
        if (!string.IsNullOrWhiteSpace(error))
        {
            log += $"{Environment.NewLine}{error.Trim()}";
        }

        return $"{log}{Environment.NewLine}Exit code: {process.ExitCode}";
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GlobalMemoryStatusEx(ref MEMORYSTATUSEX lpBuffer);

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
