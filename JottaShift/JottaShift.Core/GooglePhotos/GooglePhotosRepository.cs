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

        List<string> uploadTokens = [];
        foreach (var photoPath in photosFullPaths)
        {
            var tokenResult = await _googlePhotosClient.UploadPhotoAsync(photoPath);
            if (tokenResult.Succeeded && tokenResult.Value is not null)
            {
                uploadTokens.Add(tokenResult.Value);
            }
            else
            {
                _logger.LogError("Failed to upload image {ImagePath} to Google Photos. Error: {ErrorMessage}",
                    photoPath, tokenResult.ErrorMessage);
            }
        }

        if (uploadTokens.Count == 0)
        {
            return Result<int>.Failure("Failed to upload any photos");
        }

        var batchCreateResult = await _photosLibraryFacade.AddImagesToAlbum(albumResult.Value.Id, uploadTokens);
        if (!batchCreateResult.Succeeded || batchCreateResult.Value is null)
        {
            _logger.LogError("Failed to add images to album {AlbumName}. Error: {ErrorMessage}", albumName, batchCreateResult.ErrorMessage);
            return Result<int>.Failure("Failed to add images to album");
        }

        return Result<int>.Success(batchCreateResult.Value.NewMediaItemResults.Count);
    }
}
