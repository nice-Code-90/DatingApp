using System;
using System.IO;
using DatingApp.Application.DTOs;
using DatingApp.Application.Helpers;
using DatingApp.Application.Interfaces;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;

namespace DatingApp.Infrastructure.Services;
public class PhotoService : IPhotoService
{
    private readonly Cloudinary _cloudinary;
    public PhotoService(IOptions<CloudinarySettings> config)
    {
        var account = new Account(
        config.Value.CloudName,
        config.Value.ApiKey,
        config.Value.ApiSecret);

        _cloudinary = new Cloudinary(account);
    }
    public async Task<bool> DeletePhotoAsync(string publicId)
    {
        var deleteParams = new DeletionParams(publicId);

        var result = await _cloudinary.DestroyAsync(deleteParams);

        return result.Result == "ok";
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
            return null;
        }

        return new PhotoUploadResult
        {
            PublicId = uploadResult.PublicId,
            Url = uploadResult.SecureUrl.ToString()
        };
    }
}
