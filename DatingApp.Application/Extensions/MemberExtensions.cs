using System.Linq.Expressions;
using DatingApp.Application.DTOs;
using DatingApp.Domain.Entities;

namespace DatingApp.Application.Extensions;

public static class MemberExtensions
{
    public static Expression<Func<Member, MemberDto>> ToDtoProjection()
    {
        return m => new MemberDto
        {
            Id = m.Id,
            DisplayName = m.DisplayName,
            Age = m.DateOfBirth.CalculateAge(),
            Gender = m.Gender,
            Description = m.Description,
            City = m.City,
            Country = m.Country,
            Created = m.Created,
            LastActive = m.LastActive,
            ImageUrl = m.ImageUrl,
            Photos = m.Photos
                .Where(p => p.IsApproved)
                .AsQueryable()
                .Select(PhotoExtensions.ToDtoProjection())
                .ToList()
        };
    }
}