namespace JottaShift.Core.Jottacloud;

public interface IJottacloudRepository
{
    Task<IEnumerable<string>> GetImagesInAlbumAsync(string albumId);
    Task<string> GetImageFilePathFromFileName(string imageFileName);
}
