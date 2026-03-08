namespace JottaShift.Core.Jottacloud;

public static class JottacloudAdapter
{
    public static string AlbumIdFromSharedUri(Uri sharedUri)
    {
        return sharedUri.Segments.Last();
    }

    public static DateTimeOffset PhotoCapturedDateToLocalDateTime(Photo photo)
    {
        return DateTimeOffset.FromUnixTimeMilliseconds(photo.CapturedDate);
    }
}
