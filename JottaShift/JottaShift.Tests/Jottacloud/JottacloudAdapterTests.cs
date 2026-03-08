using JottaShift.Core.Jottacloud;

namespace JottaShift.Tests.Jottacloud;

public class JottacloudAdapterTests
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
        var expectedLocalDateTime = DateTime.Parse("22/07/2020 16:38:24"); 

        var photo = new Photo()
        {
            CapturedDate = milliseconds
        };

        var result = JottacloudAdapter.PhotoCapturedDateToLocalDateTime(photo);

        Assert.Equal(expectedLocalDateTime.Date, result.Date);
    }
}
