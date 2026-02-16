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

    public async Task<long> ClearTempFilesAsync()
    {
        return await Task.Run(() =>
        {
            string tempPath = Path.GetTempPath();
            long deletedBytes = 0;

            foreach (string file in Directory.EnumerateFiles(tempPath, "*", SearchOption.AllDirectories))
            {
                try
                {
                    FileInfo info = new(file);
                    deletedBytes += info.Length;
                    info.Attributes = FileAttributes.Normal;
                    info.Delete();
                }
                catch
                {
                    // Skip locked/unavailable files.
                }
            }

            foreach (string dir in Directory.EnumerateDirectories(tempPath, "*", SearchOption.AllDirectories).OrderByDescending(d => d.Length))
            {
                try
                {
                    Directory.Delete(dir, false);
                }
                catch
                {
                    // Skip non-empty or locked directories.
                }
            }

            return deletedBytes;
        });
    }

    public async Task<IReadOnlyList<StartupItem>> GetStartupAppsAsync()
    {
        return await Task.Run(() =>
        {
            var items = new List<StartupItem>();

            using (RegistryKey? key = Registry.CurrentUser.OpenSubKey(StartupRegistryPath, writable: false))
            {
                if (key is not null)
                {
                    foreach (string name in key.GetValueNames())
                    {
                        string command = key.GetValue(name)?.ToString() ?? string.Empty;
                        items.Add(new StartupItem
                        {
                            Name = name,
                            Source = "Registry",
                            Command = command
                        });
                    }
                }
            }

            string startupFolder = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            if (Directory.Exists(startupFolder))
            {
                foreach (string file in Directory.EnumerateFiles(startupFolder))
                {
                    items.Add(new StartupItem
                    {
                        Name = Path.GetFileNameWithoutExtension(file),
                        Source = "Startup Folder",
                        Command = file
                    });
                }
            }

            return (IReadOnlyList<StartupItem>)items.OrderBy(i => i.Name).ToList();
        });
    }

    public async Task<bool> DisableStartupAppAsync(StartupItem item)
    {
        return await Task.Run(() =>
        {
            if (item.Source == "Registry")
            {
                using RegistryKey? key = Registry.CurrentUser.OpenSubKey(StartupRegistryPath, writable: true);
                if (key is null)
                {
                    return false;
                }

                key.DeleteValue(item.Name, throwOnMissingValue: false);
                return true;
            }

            if (item.Source == "Startup Folder" && File.Exists(item.Command))
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
        {
            "ipconfig /flushdns",
            "netsh winsock reset"
        };

        var logs = new List<string>();
        foreach (string command in commands)
        {
            logs.Add(await RunShellCommandAsync(command));
        }

        return string.Join(Environment.NewLine + Environment.NewLine, logs);
    }

    public async Task<string> EnableGamingModeAsync()
    {
        return await RunShellCommandAsync("powercfg /setactive SCHEME_MIN");
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

        if (!string.IsNullOrWhiteSpace(error))
        {
            return $"> {command}{Environment.NewLine}{error.Trim()}";
        }

        return $"> {command}{Environment.NewLine}{output.Trim()}";
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
