namespace JottaShift.Core.Jottacloud;

public interface IJottacloudRepository
{
    Task<IEnumerable<AlbumItem>> GetAlbumImages(string albumId);
}
