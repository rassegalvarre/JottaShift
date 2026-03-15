using JottaShift.Core.Jottacloud.Models.Domain;
using System.Globalization;
using System.Text.RegularExpressions;

namespace JottaShift.Core.Jottacloud;

/// <summary>
/// Provides utility methods for adapting and transforming data from Jottacloud.
/// </summary>
public static partial class JottacloudAdapter
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

    /// <summary>
    /// Remove the conflict-tag of a filename if it exists..
    /// </summary>
    /// <remarks>
    /// Jottacloud will tag conflicted files with (Conflict...) in the filename.
    /// Example filename marked as conflict: P_20250411_135044 (Conflict 2025-04-11 19.15.54).jpg"</remarks>
    /// <returns></returns>
    public static string CheckAndCleanConflictedFileName(string fileName)
    {
        if (fileName.Contains("conflict", StringComparison.CurrentCultureIgnoreCase))
        {
            return ConflictedFileNameRegex().Replace(fileName, "");
        }

        return fileName;
    }

    [GeneratedRegex(@"\s*\([^)]*\)")]
    private static partial Regex ConflictedFileNameRegex();
}
