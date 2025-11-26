using System;
using System.IO;
using DatingApp.Application.DTOs;

namespace DatingApp.Application.Interfaces;

public interface IPhotoService
{
    Task<PhotoUploadResult?> UploadPhotoAsync(Stream fileStream, string fileName);
    Task<bool> DeletePhotoAsync(string publicId);
    Task<bool> DeleteMemberPhotoAsync(string memberId, int photoId);

}
