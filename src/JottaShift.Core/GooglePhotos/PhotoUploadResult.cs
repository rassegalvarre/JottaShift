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
    public static IEnumerable<string> ExtractValidUploadTokens(this IEnumerable<PhotoUploadResult> results)
    {
        return results
            .Where(r => r.UploadToken != null)
            .Select(r => r.UploadToken!);
    }
}