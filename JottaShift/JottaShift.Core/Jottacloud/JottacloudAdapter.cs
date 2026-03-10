using System.Globalization;
using JottaShift.Core.Jottacloud.Models.Domain;

namespace JottaShift.Core.Jottacloud;

/// <summary>
/// Provides utility methods for adapting and transforming data from Jottacloud.
/// </summary>
public static class JottacloudAdapter
{
    public static readonly CultureInfo DefaultCulture = CultureInfo.InvariantCulture;

    public static string AlbumIdFromSharedUri(Uri sharedUri)
    {
        return sharedUri.Segments.Last();
    }

    public static DateTimeOffset PhotoCapturedDateToLocalDateTime(Photo photo)
    {
        return DateTimeOffset.FromUnixTimeMilliseconds(photo.CapturedDate);
    }

    public static string GetMonthDirectoryName(int month, CultureInfo cultureInfo)
    {
        if (month < 1 || month > 12)
            throw new ArgumentOutOfRangeException(nameof(month), "Month must be between 1 and 12");

        int monthIndex = month - 1;

        string monthName = cultureInfo.DateTimeFormat.MonthNames[monthIndex];
        string capitalizedMonthName = char.ToUpper(monthName[0]) + monthName[1..];

        return $"{monthIndex + 1:D2} {capitalizedMonthName}";
    }

    public static string PhotoStorageStructuredDirectoryPath(
        DateTimeOffset photoCaputedDato,
        string storagePath,
        CultureInfo cultureInfo)
    {
        string year = photoCaputedDato.Year.ToString();
        string monthDirectoryName = GetMonthDirectoryName(photoCaputedDato.Month, cultureInfo);

        string structuredDirectory = Path.Combine(
            storagePath,
            year,
            monthDirectoryName);

        return Path.Combine(storagePath, structuredDirectory);
    }
}
