namespace JottaShift.Core.Jottacloud;

public interface IJottacloudRepository
{
    Task<IEnumerable<PhotoDto>> GetAlbumImages(string albumId);
}
