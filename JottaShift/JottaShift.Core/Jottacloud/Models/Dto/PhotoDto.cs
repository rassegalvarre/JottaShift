using JottaShift.Core.Jottacloud.Models.Domain;

namespace JottaShift.Core.Jottacloud.Models.Dto;

public record PhotoDto
{
    public string ImageName { get; init; }
    public DateTimeOffset CapturedDate { get; init; }
    public string? LocalFilePath { get; set; }

    public PhotoDto(Photo photo)
    {
        ImageName = photo.Filename;
        CapturedDate = JottacloudAdapter.PhotoCapturedDateToLocalDateTime(photo);
    }
}