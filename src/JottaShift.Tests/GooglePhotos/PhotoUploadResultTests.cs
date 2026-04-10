using JottaShift.Core.GooglePhotos;

namespace JottaShift.Tests.GooglePhotos;

public class PhotoUploadResultTests
{
    private static List<PhotoUploadResult> GetPhotoUploadResults(uint size, bool withValidTokens = true)
    {
        return [.. Enumerable.Range(0, (int)size)
            .Select(i => new PhotoUploadResult($"path{i}")
            {
                UploadToken = withValidTokens ? Guid.NewGuid().ToString() : null
            })];
    }

    [Fact]
    public void ExtractValidUploadTokensInChunks_ShouldReturnEmptyChunk_WhenNoValidTokens()
    {
        // Arrange
        var results = GetPhotoUploadResults(5, withValidTokens: false);

        // Act
        var tokenChunk = results.ExtractValidUploadTokensInChunks();

        // Assert
        Assert.Empty(tokenChunk);
    }

    [Fact]
    public void ExtractValidUploadTokensInChunks_ShouldReturnSingleChunk_WhenTokenCountBelowChuckSize()
    {
        // Arrange
        var results = GetPhotoUploadResults(IGooglePhotosLibraryHttpClient.MaxItemsPerCall - 1);

        // Act
        var tokenChunk = results.ExtractValidUploadTokensInChunks();

        // Assert
        Assert.Single(tokenChunk);
    }

    [Fact]
    public void ExtractValidUploadTokensInChunks_ShouldReturnChunks_WhenListIsLargerThanChunkSize()
    {
        // Arrange
        uint totalTokens = IGooglePhotosLibraryHttpClient.MaxItemsPerCall * 2 + 1;
        var results = GetPhotoUploadResults(totalTokens);

        // Act
        var tokenChunks = results.ExtractValidUploadTokensInChunks();

        // Assert
        int expectedChunks = 3; // Two full and one with a single token
        int expectedChunkSize = (int)IGooglePhotosLibraryHttpClient.MaxItemsPerCall;

        Assert.Equal(expectedChunks, tokenChunks.Count());
        Assert.Equal(expectedChunkSize, tokenChunks.ElementAt(0).Length);
        Assert.Equal(expectedChunkSize, tokenChunks.ElementAt(1).Length);
        Assert.Single(tokenChunks.ElementAt(2));
    }
}
