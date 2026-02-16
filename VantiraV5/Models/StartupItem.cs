namespace VantiraV5.Models;

internal sealed class StartupItem
{
    public required string Name { get; init; }

    public required string Source { get; init; }

    public required string Command { get; init; }
}
