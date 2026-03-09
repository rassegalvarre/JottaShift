namespace JottaShift.Core.Jottacloud;

public interface IJottacloudRepository
{
    Task<IEnumerable<PhotoDto>> GetAlbumPhotos(string albumId);
}
