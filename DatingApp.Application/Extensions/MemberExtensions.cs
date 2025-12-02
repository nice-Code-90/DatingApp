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

    public static MemberDto? ToDto(this Member member)
    {
        if (member == null) return null;

        return new MemberDto
        {
            Id = member.Id,
            DisplayName = member.DisplayName,
            ImageUrl = member.ImageUrl,
            Age = member.DateOfBirth.CalculateAge(),
            City = member.City,
            Country = member.Country,
            Gender = member.Gender,
            Description = member.Description,
            Created = member.Created,
            LastActive = member.LastActive,
            Photos = member.Photos?
                .Where(p => p.IsApproved)
                .Select(p => p.ToDto())
                .Where(p => p != null)
                .ToList()!
        };
    }
}