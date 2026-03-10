using JottaShift.Core.Jottacloud.Models.Domain;

namespace JottaShift.Core.Jottacloud;

/// <summary>
/// Client for accessing Jottacloud API via HTTP.
/// </summary>
public interface IJottacloudHttpClient
{
    /// <summary>
    /// Returns the photo-album with the specified ID.
    /// </summary>
    Task<Result<Album>> GetAlbumAsync(string albumId, int limit = 100);
}
