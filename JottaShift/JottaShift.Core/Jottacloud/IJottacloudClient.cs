using JottaShift.Core.Jottacloud.Models.Domain;

namespace JottaShift.Core.Jottacloud;

/// <summary>
/// Client for accessing Jottacloud API.
/// </summary>
public interface IJottacloudClient
{
    /// <summary>
    /// Returns the photo-album with the specified ID.
    /// </summary>
    Task<Result<Album>> GetAlbumAsync(string albumId, int limit = 100);
}
