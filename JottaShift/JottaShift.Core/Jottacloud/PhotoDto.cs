namespace JottaShift.Core.Jottacloud;

public record PhotoDto
{
    public string ImageName { get; init; }
    public DateTime CapturedDate { get; init; }
    public string? LocalFilePath { get; init; }

    public PhotoDto(Photo photo, string? locationFilePath = null)
    {
        ImageName = photo.Filename;
        CapturedDate = DateTimeOffset.FromUnixTimeMilliseconds(photo.CapturedDate).LocalDateTime;
        LocalFilePath = locationFilePath;
    }
}
