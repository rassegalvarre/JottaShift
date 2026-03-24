using JottaShift.Core.Jottacloud.Models.Dto;

namespace JottaShift.Core.Jottacloud;

/// <summary>
/// Defines methods for accessing and managing files and photo-albums in Jottacloud.
/// </summary>
public interface IJottacloudRepository
{
    /// <summary>
    /// Returns an album with photos and their local file paths if they exist in local storage.
    /// </summary>
    Task<Result<AlbumDto>> GetAlbumAsync(string albumId);
}
