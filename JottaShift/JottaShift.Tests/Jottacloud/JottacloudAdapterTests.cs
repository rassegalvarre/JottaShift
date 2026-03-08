using JottaShift.Core.Jottacloud;
using System;
using System.Collections.Generic;
using System.Text;

namespace JottaShift.Tests.Jottacloud;

public class JottacloudAdapterTests
{
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
