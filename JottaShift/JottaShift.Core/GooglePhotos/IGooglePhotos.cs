namespace JottaShift.Core.GooglePhotos;

public interface IGooglePhotos
{
    /// <summary>
    /// Uploads a list of images to Google Storage and adds them to an album.
    /// </summary>
    /// <param name="imagesFullPath">Full file-path for the images to be uploaded</param>
    /// <param name="albumName">Album full name</param>
    /// <returns>The number of items uploaded</returns>
    Task<int> UploadImagesToAlbum(IEnumerable<string> imagesFullPath, string albumName);
}
