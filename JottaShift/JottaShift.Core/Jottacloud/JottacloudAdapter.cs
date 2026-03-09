using System.Globalization;
using System.Runtime;

namespace JottaShift.Core.Jottacloud;

public static class JottacloudAdapter
{
    public static CultureInfo DefaultCulture = CultureInfo.InvariantCulture;

    public static string AlbumIdFromSharedUri(Uri sharedUri)
    {
        return sharedUri.Segments.Last();
    }

    public static DateTimeOffset PhotoCapturedDateToLocalDateTime(Photo photo)
    {
        return DateTimeOffset.FromUnixTimeMilliseconds(photo.CapturedDate);
    }

    //public string PhotoStorageDirectoryPath(DateTimeOffset photoCaputedDato)
    //{
    //    string year = photoCaputedDato.Year.ToString();
    //    string monthDirectoryName = GetMonthDirectoryName(photoCaputedDato.Month);

    //    string predictedDirectory = Path.Combine(
    //        _settings.PhotoStoragePath,
    //        year,
    //        monthDirectoryName);

    //    return Path.Combine(_settings.PhotoStoragePath, predictedDirectory);
    //}

    public static string GetMonthDirectoryName(int month, CultureInfo cultureInfo)
    {
        if (month < 1 || month > 12)
            throw new ArgumentOutOfRangeException(nameof(month), "Month must be between 1 and 12");

        int monthIndex = month - 1;

        string monthName = cultureInfo.DateTimeFormat.MonthNames[monthIndex];
        string capitalizedMonthName = char.ToUpper(monthName[0]) + monthName[1..];

        return $"{monthIndex + 1:D2} {capitalizedMonthName}";
    }
}
