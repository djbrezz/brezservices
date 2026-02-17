namespace VantiraV5.Models;

internal sealed class SystemSnapshot
{
    public required double CpuUsagePercent { get; init; }

    public required double RamUsedGb { get; init; }

    public required double RamTotalGb { get; init; }

    public required double RamUsagePercent { get; init; }
}
