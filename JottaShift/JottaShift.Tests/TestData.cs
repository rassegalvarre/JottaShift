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
    /// Collection of typical image filenames with dates in various formats.
    /// Supports patterns: img_YYYYMMDD, photo-YYYY-MM-DD, YYYYMMDD, etc.
    /// </summary>
    public static readonly IEnumerable<string> ImageFilenamesWithDates = new[]
    {
        // Underscore prefix with 8-digit format (img_YYYYMMDD)
        "img_20250517.jpg",
        "img_20241225.png",
        "photo_20230815.jpg",
        "picture_20220101.jpg",

        // Hyphenated format (YYYY-MM-DD)
        "vacation_2025-05-17.jpg",
        "photo_2024-12-25.png",
        "2025-05-17_family_photo.jpg",
        "2024-12-25_christmas.jpg",

        // Compact 8-digit format (YYYYMMDD)
        "20250517_beach.jpg",
        "20241225_holiday.png",
        "20230815_vacation.jpg",

        // Camera default filenames with dates
        "IMG_20250517_134210.jpg",
        "PHOTO_20241225_090000.jpg",
        "DSC_20230815_143022.jpg",

        // Complex filenames with dates in the middle
        "IMG_20250517_landscape_001.jpg",
        "photo_2025-05-17_sunset.jpg",
        "vacation_20250517_moment.jpg",

        // Alternative formats
        "pic-2025-05-17-001.jpg",
        "image_2025_05_17.jpg",
    };
}
