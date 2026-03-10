using JottaShift.Core.Jottacloud.Models.Domain;

namespace JottaShift.Core.Jottacloud.Models.Dto;

public record AlbumDto
{
    public string AlbumTitle { get; init; }
    public IEnumerable<PhotoDto> Photos { get; init; } = Enumerable.Empty<PhotoDto>();

    public AlbumDto(Album album)
    {
        AlbumTitle = album.Title;
        Photos = album.Photos.Select(photo => new PhotoDto(photo));
    }

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
