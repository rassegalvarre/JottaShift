namespace JottaShift.Core.Jottacloud;

/// <summary>
/// Defines methods for accessing and managing files and photo-albums in Jottacloud.
/// </summary>
public interface IJottacloudRepository
{
    /// <summary>
    /// Returns a list of photos in the specified album and their local file paths if they exist in local storage.
    /// </summary>
    Task<IEnumerable<PhotoDto>> GetAlbumPhotos(string albumId);
}
