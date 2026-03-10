using JottaShift.Core.Jottacloud.Models.Domain;

namespace JottaShift.Core.Jottacloud.Models.Dto;

public record AlbumDto
{
    public required string AlbumTitle { get; init; }
    public required IEnumerable<PhotoDto> Photos { get; init; } = [];

    public Result<PhotoDto> GetPhotoById(string photoId)
    {
        var photo = Photos.FirstOrDefault(p => p.Id == photoId);
        if (photo == null)
        {
            return Result<PhotoDto>.Failure("Photo not found");
        }
        return Result<PhotoDto>.Success(photo);
    }
}
