using JottaShift.Core.Jottacloud;

namespace JottaShift.Core.FileExport.Jobs;

public record ChromecastUploadJob : FileExportJob
{
    public required string SourceJottacloudAlbumSharedUrl { get; init; }
    public required string TargetGooglePhotosAlbumName { get; init; }

    public string SourceJottacloudAlbumId =>
        JottacloudAdapter.AlbumIdFromSharedUri(new Uri(SourceJottacloudAlbumSharedUrl));

}