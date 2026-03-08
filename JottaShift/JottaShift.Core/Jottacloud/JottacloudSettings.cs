namespace JottaShift.Core.Jottacloud;

public class JottacloudSettings
{
    public required string SyncFolderFullPath { get; init; }
    
    // TODO: rename to Photo to match Jottacloud API model
    public required string ImageStoragePath { get; init; }
    public required string TestAlbumId { get; init; }
    // public required string TestAlbumSharedUrl { get; init; } = "https://www.jottacloud.com/share/imjg7a52t61g"
}
