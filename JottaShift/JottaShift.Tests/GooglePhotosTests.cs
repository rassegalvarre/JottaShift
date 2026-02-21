using JottaShift.Core.GooglePhotos;
using System;
using System.Collections.Generic;
using System.Text;

namespace JottaShift.Tests;

public class GooglePhotosTests
{
    [Fact]
    public void GetPhotosLibraryService_CreatesService_WhenCredentialsFound()
    {
        var googlePhotosRepository = new GooglePhotosRepository();

        var photosLibraryService = googlePhotosRepository.GetPhotosLibraryService();

        Assert.NotNull(photosLibraryService);
    }
}
