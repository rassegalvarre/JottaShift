using JottaShift.Core.Jottacloud.Models.Domain;

namespace JottaShift.Core.Jottacloud.Models.Dto;

public record PhotoDto
{
    public string Id { get; init; }
    public string ImageName { get; init; }
    public DateTimeOffset CapturedDate { get; init; }
    public string? LocalFilePath { get; set; }

    public PhotoDto(string id, string imageName, DateTimeOffset capturedDate)
    {
        Id = id;
        ImageName = imageName;
        CapturedDate = capturedDate;
    }

    public PhotoDto(Photo photo)
    {
        Id = photo.Id;
        ImageName = photo.Filename;
        CapturedDate = JottacloudAdapter.PhotoCapturedDateToLocalDateTime(photo);
    }
}