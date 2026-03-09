namespace JottaShift.Core.Jottacloud;

public class JottacloudSettings
{
    public required string ApiUri { get; init; }
    public required string SyncFolderFullPath { get; init; }    
    public required string PhotoStoragePath { get; init; }
    public required string TestAlbumId { get; init; }
    // public required string TestAlbumSharedUrl { get; init; } = "https://www.jottacloud.com/share/imjg7a52t61g"
}
