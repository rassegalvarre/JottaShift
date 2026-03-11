namespace JottaShift.Core.GooglePhotos;

public interface IGooglePhotosRepository
{
    /// <summary>
    /// Uploads a list of photos to Google Storage and adds them to an album.
    /// </summary>
    /// <param name="photosFullPath">Full file-path for the photos to be uploaded</param>
    /// <param name="albumName">Album full name</param>
    /// <returns>A <see cref="Result[T]" with the number of images uploaded/></returns>
    Task<Result<int>> UploadPhotosToAlbum(IEnumerable<string> photosFullPath, string albumName);
}
