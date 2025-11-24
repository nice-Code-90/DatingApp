using System;
using DatingApp.Application.DTOs;
using DatingApp.Domain.Entities;


namespace DatingApp.Application.Interfaces;

public interface IPhotoRepository
{
    Task<IReadOnlyList<PhotoForApprovalDto>> GetUnapprovedPhotos();
    Task<Photo?> GetPhotoById(int id);
    void RemovePhoto(Photo photo);
}
