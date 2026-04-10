using JottaShift.Core.GooglePhotos.PhotosLibraryV1;

namespace JottaShift.Core.GooglePhotos;

public record PhotoUploadResult(string FilePath)
{
    public string FilePath { get; init; } = FilePath;
    public string? UploadToken { get; set; }
    public string? Id { get; set; }
    public string? Url { get; set; }
    public string? StatusMessage { get; set; }

    public void FromNewMediaItemResult(NewMediaItemResult result)
    {
        Id = result.MediaItem?.Id;
        Url = result.MediaItem?.ProductUrl;
        StatusMessage = result.Status?.Message;
    }
}

public static class PhotoUploadResultExtensions
{
    /// <summary>
    /// Extracts valid <see cref="PhotoUploadResult.UploadToken"/> in chunks of max allowed items per call to Google Photos API,
    /// as defined by <see cref="IGooglePhotosLibraryHttpClient.MaxItemsPerCall"/>."/>
    /// </summary>
    public static IEnumerable<string[]> ExtractValidUploadTokensInChunks(this IEnumerable<PhotoUploadResult> results)
    {
        var uploadTokens = results
            .Where(r => r.UploadToken != null)
            .Select(r => r.UploadToken!);

        if (!uploadTokens.Any())
        {
            return [];
        }

        if (uploadTokens.Count() > IGooglePhotosLibraryHttpClient.MaxItemsPerCall)
        {
            return uploadTokens.Chunk((int)IGooglePhotosLibraryHttpClient.MaxItemsPerCall);
        }
        else
        {
            return [[.. uploadTokens]];
        }
    }
}