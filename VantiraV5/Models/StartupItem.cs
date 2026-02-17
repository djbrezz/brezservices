namespace VantiraV5.Models;

/// <summary>
/// Represents a startup application entry discovered from registry or startup folders.
/// </summary>
internal sealed class StartupItem
{
    public required string Name { get; init; }

    public required string Source { get; init; }

    public required string Location { get; init; }

    public required string Command { get; init; }
}
