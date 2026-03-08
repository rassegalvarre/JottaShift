namespace JottaShift.Core.Jottacloud;

public interface IJottacloudRepository
{
    /// <exception cref="HttpRequestException" />
    Task<IEnumerable<string>> GetLocalPathForImagesInAlbumAsync(string albumId);

    /// <exception cref="HttpRequestException" />
    Task<IEnumerable<string>> GetImagesInAlbumAsync(string albumId);

    Task<string> GetImageFilePathFromFileName(string imageFileName);
}
