namespace JottaShift.Core.Jottacloud;

public interface IJottacloudClient
{
    Task<Result<Album>> GetAlbumAsync(string albumId, int limit = 100);
}
