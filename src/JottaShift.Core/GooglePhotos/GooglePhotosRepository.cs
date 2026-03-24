using Google.Apis.PhotosLibrary.v1.Data;
using Microsoft.Extensions.Logging;

namespace JottaShift.Core.GooglePhotos;

public class GooglePhotosRepository(
    IGooglePhotosLibraryFacade _photosLibraryFacade,
    IGooglePhotosHttpClient _googlePhotosClient,
    ILogger<GooglePhotosRepository> _logger) : IGooglePhotosRepository
{
    public async Task<Result<Album>> GetOrCreateAlbumAsync(string albumName)
    {
        var albumResult = await _photosLibraryFacade.GetAlbumFromTitleAsync(albumName);
        if (albumResult.Succeeded && albumResult.Value is not null)
        {
            return albumResult;
        }

        return await _photosLibraryFacade.CreateAlbumAsync(albumName);
    }
 
    public async Task<AlbumUploadResult> UploadPhotosToAlbumAsync(string albumName, IEnumerable<string> photosFullPaths)
    {
        var albumResult = await GetOrCreateAlbumAsync(albumName);
        if (!albumResult.Succeeded || albumResult.Value is null)
        {
            _logger.LogError("Failed to get or create album {AlbumName}. Error: {ErrorMessage}", albumName, albumResult.ErrorMessage);
            return AlbumUploadResult.FromFailedResult(albumResult, albumName);
        }

        if (photosFullPaths.Count() == 0)
        {
            _logger.LogWarning("No photos were provided for upload to album {AlbumName}", albumName);
            return AlbumUploadResult.Success(albumName, []);
        }

        List<PhotoUploadResult> photoUploadResults = [];
        foreach (var photoPath in photosFullPaths)
        {
            var result = new PhotoUploadResult(photoPath);

            var tokenResult = await _googlePhotosClient.UploadPhotoAsync(photoPath);
            if (tokenResult.Succeeded && tokenResult.Value is not null)
            {
                result.UploadToken = tokenResult.Value;
            }
            else
            {
                result.StatusMessage = "Could not obtain token";
                _logger.LogError("Failed to upload image {ImagePath} to Google Photos. Error: {ErrorMessage}",
                    photoPath, tokenResult.ErrorMessage);
            }
            
            photoUploadResults.Add(result);
        }

        var uploadTokens = photoUploadResults.ExtractValidUploadTokens();

        if (!uploadTokens.Any())
        {
            return AlbumUploadResult.Failure(albumName, "Failed to upload any photos", photoUploadResults);
        }

        var batchCreateResult = await _photosLibraryFacade.AddImagesToAlbum(
            albumResult.Value.Id, uploadTokens);
        if (!batchCreateResult.Succeeded || batchCreateResult.Value is null)
        {
            _logger.LogError("Failed to add images to album {AlbumName}. Error: {ErrorMessage}", albumName, batchCreateResult.ErrorMessage);
            return AlbumUploadResult.FromFailedResult(batchCreateResult, albumName, photoUploadResults);
        }
        else
        {
            foreach (var item in batchCreateResult.Value.NewMediaItemResults)
            {
                var result = photoUploadResults.First(r => r.UploadToken == item.UploadToken);
                result.FromNewMediaItemResult(item);
            }
        }

        return AlbumUploadResult.Success(albumName, photoUploadResults);
    }
}
