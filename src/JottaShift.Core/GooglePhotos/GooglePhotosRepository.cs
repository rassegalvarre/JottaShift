using JottaShift.Core.GooglePhotos.PhotosLibraryV1;
using Microsoft.Extensions.Logging;

namespace JottaShift.Core.GooglePhotos;

public class GooglePhotosRepository(
    IGooglePhotosLibraryHttpClient _googlePhotosClient,
    ILogger<GooglePhotosRepository> _logger) : IGooglePhotosRepository
{
    public async Task<Result<Album>> GetOrCreateAlbumAsync(string albumTitle)
    {
        var albumResult = await _googlePhotosClient.GetAlbumFromTitleAsync(albumTitle);
        if (albumResult.Succeeded && albumResult.Value is not null)
        {
            return albumResult;
        }

        return await _googlePhotosClient.CreateAlbumAsync(albumTitle);
    }
 
    public async Task<AlbumUploadResult> UploadPhotosToAlbumAsync(string albumName, IEnumerable<string> photosFullPaths)
    {
        var albumResult = await GetOrCreateAlbumAsync(albumName);
        if (!albumResult.Succeeded || albumResult.Value?.Id is null)
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

            var tokenResult = await _googlePhotosClient.UploadMediaAsync(photoPath);
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

        var batchCreateResult = await _googlePhotosClient.BatchAddMediaItemsAsync(
            albumResult.Value.Id, uploadTokens);
        if (!batchCreateResult.Succeeded || batchCreateResult.Value?.NewMediaItemResults is null)
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
