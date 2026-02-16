namespace VantiraV5.Models;

/// <summary>
/// Captures cleanup execution details for user-facing reporting.
/// </summary>
internal sealed class CleanupResult
{
    public required long BytesFreed { get; init; }

    public required int FilesDeleted { get; init; }

    public required int DirectoriesDeleted { get; init; }

    public required int Failures { get; init; }
}
