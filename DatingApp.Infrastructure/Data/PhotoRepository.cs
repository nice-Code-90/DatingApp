using System;
using DatingApp.Application.DTOs;
using DatingApp.Domain.Entities;
using DatingApp.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.Infrastructure.Data;

public class PhotoRepository(AppDbContext context) : IPhotoRepository
{
    public async Task<Photo?> GetPhotoById(int id)
    {
        return await context.Photos
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(x => x.Id == id);
    }

    public async Task<IReadOnlyList<PhotoForApprovalDto>> GetUnapprovedPhotos()
    {
        return await context.Photos
            .IgnoreQueryFilters()
            .Where(p => p.IsApproved == false)
            .Select(u => new PhotoForApprovalDto
            {
                Id = u.Id,
                UserId = u.MemberId,
                Url = u.Url,
                IsApproved = u.IsApproved
            }).ToListAsync();
    }

    public void RemovePhoto(Photo photo)
    {
        context.Photos.Remove(photo);
    }
}