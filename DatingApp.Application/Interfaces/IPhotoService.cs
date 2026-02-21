using DatingApp.Application.DTOs;

namespace DatingApp.Application.Interfaces;

public interface IPhotoService
{
    Task<PhotoUploadResultDto?> UploadPhotoAsync(Stream fileStream, string fileName);
    Task<bool> DeletePhotoAsync(string publicId);
    Task<bool> DeleteMemberPhotoAsync(string memberId, int photoId);

}
