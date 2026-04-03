using Google.Apis.PhotosLibrary.v1;
using Google.Apis.PhotosLibrary.v1.Data;
using Google.Apis.Services;
using Microsoft.Extensions.Logging;

namespace JottaShift.Core.GooglePhotos;

public class GooglePhotosLibraryFacade : IGooglePhotosLibraryFacade
{
    private readonly ILogger<GooglePhotosLibraryFacade> _logger;
    private readonly IUserCredentialManager _userCredentialManager;
    private readonly Lazy<Task<PhotosLibraryService>> _photoLibraryService;

    public GooglePhotosLibraryFacade(
        IUserCredentialManager userCredentialManager,
        ILogger<GooglePhotosLibraryFacade> logger)
    {
        _logger = logger;
        _userCredentialManager = userCredentialManager;
        _photoLibraryService = new Lazy<Task<PhotosLibraryService>>(InitializeServiceAsync());
    }

  
    private async Task<PhotosLibraryService> InitializeServiceAsync()
    {
        _logger.LogInformation("Initializing PhotosLibraryService");

        var credential = await _userCredentialManager.GetUserCredentialAsync();
        if (!credential.Succeeded)
        {
            _logger.LogError("Failed to obtain user credentials: {ErrorMessage}", credential.ErrorMessage);
            throw new Exception();
        }

        var service = new PhotosLibraryService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential.Value,
            ApplicationName = "JottaShift"
        });

        _logger.LogInformation("PhotosLibraryService initialized successfully");
        return service;
    }

    private async Task<PhotosLibraryService> GetServiceAsync()
    {
        _logger.LogDebug("Getting PhotosLibraryService");
        return await _photoLibraryService.Value;
    }

    private async Task<Result<ListAlbumsResponse>> GetAlbums()
    {
        try
        {
            var photosLibraryService = await GetServiceAsync();

            var albumListResponse = await photosLibraryService.Albums.List().ExecuteAsync();

            return albumListResponse != null 
                ? Result<ListAlbumsResponse>.Success(albumListResponse)
                : Result<ListAlbumsResponse>.Failure("Albums not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get albums");
            return Result<ListAlbumsResponse>.Failure("An exception occurred");
        }
    }

    public async Task<Result<Album>> GetAlbumFromTitleAsync(string albumName)
    {
        var albumsResult = await GetAlbums();
        if (!albumsResult.Succeeded || albumsResult.Value is null)
        {
            return Result<Album>.Failure(albumsResult.ErrorMessage ?? "Failed to get albums");
        }

        var album = albumsResult.Value.Albums?.FirstOrDefault(a => a.Title == albumName);
        if (album is null)
        {
            return Result<Album>.Failure($"Album with name '{albumName}' not found");
        }

        return Result<Album>.Success(album);
    }

    public async Task<Result<Album>> GetAlbumFromIdAsync(string albumId)
    {
        var albumsResult = await GetAlbums();
        if (!albumsResult.Succeeded || albumsResult.Value is null)
        {
            return Result<Album>.Failure(albumsResult.ErrorMessage ?? "Failed to get albums");
        }

        var album = albumsResult.Value.Albums?.FirstOrDefault(a => a.Id == albumId);
        if (album is null)
        {
            return Result<Album>.Failure($"Album with Id '{albumId}' not found");
        }

        return Result<Album>.Success(album);
    }

    public async Task<Result<Album>> CreateAlbumAsync(string albumTitle)
    {
        try
        {
            var photosLibraryService = await GetServiceAsync();

            var request = new CreateAlbumRequest
            {
                Album = new Album()
                {
                    Title = albumTitle
                }
            };
            var newAlbumResponse = await photosLibraryService.Albums.Create(request).ExecuteAsync();

            if (newAlbumResponse is null)
            {
                _logger.LogWarning("Failed to created new album with title {AlbumTitle}", albumTitle);
                return Result<Album>.Failure("Album was not created");
            }

            return Result<Album>.Success(newAlbumResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create new album {AlbumName}", albumTitle);
            return Result<Album>.Failure("An exception occurred");
        }
    }

    public async Task<Result<BatchCreateMediaItemsResponse>> AddImagesToAlbum(string albumId, IEnumerable<string> uploadTokens)
    {
        try
        {
            var photosLibraryService = await GetServiceAsync();

            var batchCreateRequest = new BatchCreateMediaItemsRequest
            {
                AlbumId = albumId,
                NewMediaItems = [.. uploadTokens.Select(token => new NewMediaItem
                {
                    SimpleMediaItem = new SimpleMediaItem { UploadToken = token }
                })]
            };

            var batchCreateResponse = await photosLibraryService.MediaItems
                .BatchCreate(batchCreateRequest)
                .ExecuteAsync();

            if (batchCreateResponse == null || batchCreateResponse.NewMediaItemResults == null)
            {
                return Result<BatchCreateMediaItemsResponse>.Failure("Failed to add media items to album");
            }

            _logger.LogInformation("Uploaded {ItemCount} items to Google Photos album with Id {AlbumId}",
                batchCreateResponse.NewMediaItemResults.Count, albumId);

            return Result<BatchCreateMediaItemsResponse>.Success(batchCreateResponse);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Failed to add images to album with title {AlbumId}", albumId);
            return Result<BatchCreateMediaItemsResponse>.Failure("An exception occurred");
        }
    }
}
