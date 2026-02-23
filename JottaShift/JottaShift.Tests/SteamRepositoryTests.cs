using System;
using System.Collections.Generic;
using System.Text;
using JottaShift.Core.SteamRepository;

namespace JottaShift.Tests;

public class SteamRepositoryTests
{
    [Fact]
    public async Task GetGameName_FromId_WhenValidId()
    {
        var steamRepository = new SteamRepository();

        const uint appId = 990080;
        const string appName = "Hogwarts Legacy";

        var result = await steamRepository.GetAppNameFromId(appId);

        Assert.Equal(appName, result);
    }
}
