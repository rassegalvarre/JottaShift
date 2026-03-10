namespace JottaShift.Core.Jottacloud;

public class JottacloudSettings
{
    public required string ApiUri { get; init; }
    public required string SyncFolderFullPath { get; init; }    
    public required string PhotoStoragePath { get; init; }
    public required string TestAlbumId { get; init; }
}
