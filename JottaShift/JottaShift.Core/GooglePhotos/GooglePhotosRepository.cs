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
 
    // TODO: Return new Result with List<PhotoUploadResult>
    public async Task<Result<int>> UploadPhotosToAlbumAsync(string albumName, IEnumerable<string> photosFullPaths)
    {
        var albumResult = await GetOrCreateAlbumAsync(albumName);
        if (!albumResult.Succeeded || albumResult.Value is null)
        {
            _logger.LogError("Failed to get or create album {AlbumName}. Error: {ErrorMessage}", albumName, albumResult.ErrorMessage);
            return Result<int>.Failure("Could not find album");
        }

        if (photosFullPaths.Count() == 0)
        {
            _logger.LogWarning("No photos were provided for upload to album {AlbumName}", albumName);
            return Result<int>.Success(0);
        }

        List<PhotoUploadResult> results= [];
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
            
            results.Add(result);
        }

        if (results.Count == 0)
        {
            return Result<int>.Failure("Failed to upload any photos");
        }

        var uploadTokens = results
            .Where(r => r.UploadToken != null)
            .Select(r => r.UploadToken!);

        var batchCreateResult = await _photosLibraryFacade.AddImagesToAlbum(
            albumResult.Value.Id, uploadTokens);
        if (!batchCreateResult.Succeeded || batchCreateResult.Value is null)
        {
            _logger.LogError("Failed to add images to album {AlbumName}. Error: {ErrorMessage}", albumName, batchCreateResult.ErrorMessage);
            return Result<int>.Failure("Failed to add images to album");
        }
        else
        {
            foreach (var item in batchCreateResult.Value.NewMediaItemResults)
            {
                var result = results.First(r => r.UploadToken == item.UploadToken);
                result.FromNewMediaItemResult(item);
            }
        }

        // Return Path, Status where token is match-key
        return Result<int>.Success(batchCreateResult.Value.NewMediaItemResults.Count);
    }
}
