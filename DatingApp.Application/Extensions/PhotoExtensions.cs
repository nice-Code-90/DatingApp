using System.Linq.Expressions;
using DatingApp.Application.DTOs;
using DatingApp.Domain.Entities;

namespace DatingApp.Application.Extensions;

public static class PhotoExtensions
{
    public static Expression<Func<Photo, PhotoDto>> ToDtoProjection()
    {
        return p => new PhotoDto
        {
            Id = p.Id,
            Url = p.Url,
            IsMain = p.Member.ImageUrl == p.Url,
            IsApproved = p.IsApproved
        };
    }

    public static PhotoDto? ToDto(this Photo photo)
    {
        if (photo == null) return null;

        return new PhotoDto
        {
            Id = photo.Id,
            Url = photo.Url,
            IsMain = photo.Member?.ImageUrl == photo.Url,
            IsApproved = photo.IsApproved
        };
    }
}