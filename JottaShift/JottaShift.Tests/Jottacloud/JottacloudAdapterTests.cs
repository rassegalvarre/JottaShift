using JottaShift.Core.Jottacloud;
using JottaShift.Core.Jottacloud.Models.Domain;
using System.Globalization;

namespace JottaShift.Tests.Jottacloud;

public class JottacloudAdapterTests(JottacloudFixture _fixture)
    : IClassFixture<JottacloudFixture>
{
    [Fact]
    public void AlbumIdFromSharedUri_ReturnsAlbumId()
    {
        const string albumId = "mse9823jff12fver";
        var sharedUri = new Uri($"https://www.jottacloud.com/share/{albumId}");

        var result = JottacloudAdapter.AlbumIdFromSharedUri(sharedUri);

        Assert.Equal(albumId, result);
    }

    [Fact]
    public void PhotoCapturedDateToLocalDateTime_ParsesLongToDateTime()
    {
        long milliseconds = 1595431104000;
        var expectedLocalDateTime = DateTime.ParseExact(
            "22/07/2020 16:38:24",
            "dd/MM/yyyy HH:mm:ss", 
            CultureInfo.InvariantCulture); 

        var photo = new Photo()
        {
            CapturedDate = milliseconds
        };

        var result = JottacloudAdapter.PhotoCapturedDateToLocalDateTime(photo);

        Assert.Equal(expectedLocalDateTime.Date, result.Date);
    }

    [Theory]
    [InlineData(1, "01 January")]
    [InlineData(2, "02 February")]
    [InlineData(3, "03 March")]
    [InlineData(4, "04 April")]
    [InlineData(5, "05 May")]
    [InlineData(6, "06 June")]
    [InlineData(7, "07 July")]
    [InlineData(8, "08 August")]
    [InlineData(9, "09 September")]
    [InlineData(10, "10 October")]
    [InlineData(11, "11 November")]
    [InlineData(12, "12 December")]
    public void GetMonthDirectoryName_GetsMonthOrderAndName(int month, string expected)
    {
        var culture = JottacloudAdapter.DefaultCulture;
        var directoryName = JottacloudAdapter.GetMonthDirectoryName(month, culture);

        Assert.Equal(expected, directoryName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(13)]
    public void GetMonthDirectoryName_ThrowsWhenInvalidMotn(int month)
    {
        var culture = JottacloudAdapter.DefaultCulture;
        Assert.Throws<ArgumentOutOfRangeException>(
            () => JottacloudAdapter.GetMonthDirectoryName(month, culture));
    }

    [Theory]
    [InlineData(2011, 1, 1, @"2011\01 January")]
    [InlineData(2012, 5, 17, @"2012\05 May")]
    [InlineData(2013, 12, 31, @"2013\12 December")]
    public async Task PhotoStorageStructuredDirectoryPath_CreatesStructuredPath(
        int year,
        int month,
        int day,
        string expectedStructuredDirectory)
    {
        var caputuredDate = new DateTimeOffset(year, month, day, 12, 12, 12, TimeSpan.Zero);
        var appSettings = await _fixture.GetAppSettingsAsync();
        
        var cultureInfo = JottacloudAdapter.DefaultCulture;

        var directory = JottacloudAdapter.PhotoStorageStructuredDirectoryPath(
            caputuredDate, 
            appSettings.JottacloudSettings.PhotoStoragePath,
            cultureInfo);

        Assert.EndsWith(expectedStructuredDirectory, directory);
    }

    [Theory]
    [InlineData("no_conflict.pdf", "no_conflict.pdf")]
    [InlineData("VID_20250411(Conflict).mp4", "VID_20250411.mp4")]
    [InlineData("DSC06380 (Conflict 2025-04-11 19.15.54).JPG", "DSC06380.JPG")]
    [InlineData("P_20250411_135044 (Conflict 2025-04-11 19.15.54).jpg", "P_20250411_135044.jpg")]
    [InlineData("V_20250411_135044 (Conflict 2025-04-11 19.15.54).mp4", "V_20250411_135044.mp4")]
    [InlineData("IMG_20140325_113330 (Conflict 2025-04-11 19.15.54).jpg", "IMG_20140325_113330.jpg")]
    [InlineData(
        "Some Word document(BB's conflicted copy 2024-07-05).docx",
        "Some Word document.docx")]
    public void CheckAndHandleConflictedFileName_ShouldCleanConflicts(
        string conflictedFileName,
        string expectedFileName)
    {
        var cleaned = JottacloudAdapter.CheckAndCleanConflictedFileName(conflictedFileName);

        Assert.Equal(expectedFileName, cleaned);
    }
}
