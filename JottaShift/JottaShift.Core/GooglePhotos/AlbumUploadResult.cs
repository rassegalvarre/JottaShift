namespace JottaShift.Core.GooglePhotos;

public record AlbumUploadResult : Result
{
    public string AlbumName { get; private init; }

    public IEnumerable<PhotoUploadResult> PhotoUploadResults { get; private set; } = [];

    private AlbumUploadResult(string albumName)
    {
        AlbumName = albumName;
    }

    public static AlbumUploadResult Success(string albumName, IEnumerable<PhotoUploadResult> uploadResults)
    {
        bool allUploadsSucceeded;
        if (uploadResults.Any())
        {
            allUploadsSucceeded = uploadResults.All(r => r.StatusMessage == "Success");
        }
        else
        {
            allUploadsSucceeded = true;
        }
        

        return new AlbumUploadResult(albumName)
        {
            PhotoUploadResults = uploadResults,
            Succeeded = allUploadsSucceeded,
            ErrorMessage = !allUploadsSucceeded ? "Some failed to upload" : null
        };
    }

    public static AlbumUploadResult Failure(string albumName, string errorMessage)
    {
        return new AlbumUploadResult(albumName)
        {
            Succeeded = false,
            ErrorMessage = errorMessage
        };
    }

    public static AlbumUploadResult Failure(string albumName, string errorMessage, IEnumerable<PhotoUploadResult> uploadResults)
    {
        return Failure(albumName, errorMessage) with
        {
            PhotoUploadResults = uploadResults
        };
    }

    public static AlbumUploadResult FromFailedResult(Result result, string albumName)
    {
        return Failure(albumName, result.ErrorMessage ?? "Failure");
    }

    public static AlbumUploadResult FromFailedResult(Result result, string albumName, IEnumerable<PhotoUploadResult> uploadResults)
    {
        return Failure(albumName, result.ErrorMessage ?? "Failure") with
        {
            PhotoUploadResults = uploadResults
        };
    }
}
