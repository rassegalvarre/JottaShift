using Google.Apis.PhotosLibrary.v1.Data;
using Microsoft.Extensions.Logging;

namespace JottaShift.Core.GooglePhotos;

public class GooglePhotosRepository(
    IGooglePhotosLibraryFacade _photosLibraryFacade,
    IGooglePhotosHttpClient _googlePhotosClient,
    ILogger<GooglePhotosRepository> _logger) : IGooglePhotosRepository
{
    private async Task<Result<Album>> GetOrCreateAlbum(string albumName)
    {
        var albumResult = await _photosLibraryFacade.GetAlbumFromTitleAsync(albumName);
        if (albumResult.Succeeded && albumResult.Value is not null)
        {
            return albumResult;
        }

        var newAlbumResult = await _photosLibraryFacade.CreateAlbumAsync(albumName);
        return newAlbumResult;
    }
 
    public async Task<Result<int>> UploadPhotosToAlbum(IEnumerable<string> photosFullPaths, string albumName)
    {
        var albumResult = await GetOrCreateAlbum(albumName);
        if (!albumResult.Succeeded || albumResult.Value is null)
        {
            _logger.LogError("Failed to get or create album {AlbumName}. Error: {ErrorMessage}", albumName, albumResult.ErrorMessage);
            return Result<int>.Failure("Could not find album");
        }

        List<string> uploadTokens = new();
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

        var batchCreateResult = await _photosLibraryFacade.AddImagesToAlbum(albumResult.Value.Id, uploadTokens);
        if (!batchCreateResult.Succeeded || batchCreateResult.Value is null)
        {
            _logger.LogError("Failed to add images to album {AlbumName}. Error: {ErrorMessage}", albumName, batchCreateResult.ErrorMessage);
            return Result<int>.Failure("Failed to add images to album");
        }

        return Result<int>.Success(batchCreateResult.Value.NewMediaItemResults.Count);
    }
}
