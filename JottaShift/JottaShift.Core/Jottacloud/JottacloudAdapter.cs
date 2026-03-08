namespace JottaShift.Core.Jottacloud;

public static class JottacloudAdapter
{
    public static DateTimeOffset PhotoCapturedDateToLocalDateTime(Photo photo)
    {
        return DateTimeOffset.FromUnixTimeMilliseconds(photo.CapturedDate);
    }
}
