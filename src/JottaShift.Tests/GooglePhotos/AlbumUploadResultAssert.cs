using JottaShift.Core.GooglePhotos;

namespace JottaShift.Tests.GooglePhotos;

public class AlbumUploadResultAssert
{
    public static void Success(AlbumUploadResult result)
    {
        ResultAssert.Success(result);
    }

    public static void Failure(AlbumUploadResult result)
    {
        ResultAssert.Failure(result);
    }
}
