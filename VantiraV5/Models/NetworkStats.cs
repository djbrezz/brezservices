namespace VantiraV5.Models;

internal sealed class NetworkStats
{
    public required double DownloadMbps { get; init; }

    public required double UploadMbps { get; init; }

    public required long PingMs { get; init; }
}
