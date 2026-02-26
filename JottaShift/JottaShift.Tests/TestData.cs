using System;
using System.Collections.Generic;
using System.Text;

namespace JottaShift.Tests;

internal static class TestData
{
    private static readonly string TestDataPath = Path.Combine(AppContext.BaseDirectory, "TestData");
    public static readonly string Duck = Path.Combine(TestDataPath, "duck.jpg");
    public static readonly string DuckCopy = Path.Combine(TestDataPath, "duck_copy.jpg");
    public static readonly string Waterfall = Path.Combine(TestDataPath, "waterfall.jpg");

    /// <summary>
    /// Collection of typical image filenames with dates in various formats and their expected date components.
    /// Supports patterns: img_YYYYMMDD, photo-YYYY-MM-DD, YYYYMMDD, etc.
    /// </summary>
    public static readonly IEnumerable<ImageFilenameData> ImageFilenamesWithDates = new[]
    {
        // Underscore prefix with 8-digit format (img_YYYYMMDD)
        new ImageFilenameData("img_20250517.jpg", 2025, 5, 17),
        new ImageFilenameData("img_20241225.png", 2024, 12, 25),
        new ImageFilenameData("photo_20230815.jpg", 2023, 8, 15),
        new ImageFilenameData("picture_20220101.jpg", 2022, 1, 1),

        // Hyphenated format (YYYY-MM-DD)
        new ImageFilenameData("vacation_2025-05-17.jpg", 2025, 5, 17),
        new ImageFilenameData("photo_2024-12-25.png", 2024, 12, 25),
        new ImageFilenameData("2025-05-17_family_photo.jpg", 2025, 5, 17),
        new ImageFilenameData("2024-12-25_christmas.jpg", 2024, 12, 25),

        // Compact 8-digit format (YYYYMMDD)
        new ImageFilenameData("20250517_beach.jpg", 2025, 5, 17),
        new ImageFilenameData("20241225_holiday.png", 2024, 12, 25),
        new ImageFilenameData("20230815_vacation.jpg", 2023, 8, 15),

        // Camera default filenames with dates
        new ImageFilenameData("IMG_20250517_134210.jpg", 2025, 5, 17),
        new ImageFilenameData("PHOTO_20241225_090000.jpg", 2024, 12, 25),
        new ImageFilenameData("DSC_20230815_143022.jpg", 2023, 8, 15),

        // Complex filenames with dates in the middle
        new ImageFilenameData("IMG_20250517_landscape_001.jpg", 2025, 5, 17),
        new ImageFilenameData("photo_2025-05-17_sunset.jpg", 2025, 5, 17),
        new ImageFilenameData("vacation_20250517_moment.jpg", 2025, 5, 17),

        // Alternative formats
        new ImageFilenameData("pic-2025-05-17-001.jpg", 2025, 5, 17),
    };

    public record ImageFilenameData(string Filename, int Year, int Month, int Day);
}
