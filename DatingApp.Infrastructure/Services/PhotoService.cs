using DatingApp.Application.DTOs;
using DatingApp.Application.Helpers;
using DatingApp.Application.Interfaces;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace DatingApp.Infrastructure.Services;
public class PhotoService : IPhotoService
{
    private readonly Cloudinary _cloudinary;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<PhotoService> _logger;
    public PhotoService(IOptions<CloudinarySettings> config, ILogger<PhotoService> logger, IUnitOfWork uow)
    {
        var account = new Account(
        config.Value.CloudName,
        config.Value.ApiKey,
        config.Value.ApiSecret);
        _cloudinary = new Cloudinary(account);
        _uow = uow;
        _logger = logger;
    }
    public async Task<bool> DeletePhotoAsync(string publicId)
    {
        var deleteParams = new DeletionParams(publicId);

        var result = await _cloudinary.DestroyAsync(deleteParams);

        if (result.Result != "ok")
        {
            _logger.LogError("Failed to delete photo from Cloudinary. Result: {Result}", result.Result);
            return false;
        }
        return true;
    }

    public async Task<PhotoUploadResult?> UploadPhotoAsync(Stream fileStream, string fileName)
    {
        if (fileStream.Length == 0) return null;

        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(fileName, fileStream),
            Transformation = new Transformation().Height(500).Width(500).Crop("fill").Gravity("face"),
            Folder = "da-ang20"
        };

        var uploadResult = await _cloudinary.UploadAsync(uploadParams);

        if (uploadResult.Error != null)
        {
            _logger.LogError(uploadResult.Error.Message, "Failed to upload photo to Cloudinary.");
            return null;
        }

        return new PhotoUploadResult
        {
            PublicId = uploadResult.PublicId,
            Url = uploadResult.SecureUrl.ToString()
        };
    }

    public async Task<bool> DeleteMemberPhotoAsync(string memberId, int photoId)
    {
        var member = await _uow.MemberRepository.GetMemberForUpdate(memberId);
        if (member == null) return false;

        var photo = member.Photos.SingleOrDefault(x => x.Id == photoId);
        if (photo == null || photo.Url == member.ImageUrl)
        {
            return false; 
        }

        if (photo.PublicId != null)
        {
            var result = await DeletePhotoAsync(photo.PublicId);
            if (!result) return false; 
        }

        member.Photos.Remove(photo);

        return await _uow.Complete();
    }
}
