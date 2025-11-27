using DatingApp.Application.DTOs;
using DatingApp.Application.Helpers;
using DatingApp.Application.Interfaces;
using DatingApp.Domain.Entities;
using DatingApp.Infrastructure.Data;
using NetTopologySuite.Geometries;

namespace DatingApp.Infrastructure.Services;

public class MemberService(IUnitOfWork uow, IGeocodingService geocodingService, ICacheService cacheService) : IMemberService
{
    public async Task<bool> SetMainPhotoAsync(string memberId, int photoId)
    {
        var member = await uow.MemberRepository.GetMemberForUpdate(memberId);
        if (member == null) return false;

        var photo = member.Photos.SingleOrDefault(x => x.Id == photoId);
        if (photo == null || photo.Url == member.ImageUrl) return false;

        member.ImageUrl = photo.Url;
        member.User.ImageUrl = photo.Url;

        var result = await uow.Complete();
        if (result) await cacheService.RemoveByPrefixAsync("members:");

        return result;
    }

    public async Task<bool> UpdateMemberAsync(string memberId, MemberUpdateDto memberUpdateDto)
    {
        var member = await uow.MemberRepository.GetMemberForUpdate(memberId);
        if (member == null) return false;

        var originalCity = member.City;
        var originalCountry = member.Country;

        member.DisplayName = memberUpdateDto.DisplayName ?? member.DisplayName;
        member.Description = memberUpdateDto.Description ?? member.Description;
        member.City = memberUpdateDto.City ?? member.City;
        member.Country = memberUpdateDto.Country ?? member.Country;
        member.User.DisplayName = memberUpdateDto.DisplayName ?? member.User.DisplayName;

        if (member.City != originalCity || member.Country != originalCountry)
        {
            member.Location = await geocodingService.GetCoordinatesForAddressAsync(member.City, member.Country);
        }

        uow.MemberRepository.Update(member);
        var result = await uow.Complete();
        if (result) await cacheService.RemoveByPrefixAsync("members:");

        return result;
    }

    public async Task<PaginatedResult<Member>> GetMembersWithFiltersAsync(MemberParams memberParams, Point? currentUserLocation)
    {
        var cacheKey = $"members:{memberParams.PageNumber}-{memberParams.PageSize}:{memberParams.Gender}:{memberParams.MinAge}-{memberParams.MaxAge}:{memberParams.OrderBy}:{memberParams.Distance}";
        
        var cachedResult = await cacheService.GetAsync<PaginatedResult<Member>>(cacheKey);

        if (cachedResult != null) return cachedResult;

        var query = uow.MemberRepository.GetMembersAsQueryable();

        query = query.Where(x => x.Id != memberParams.CurrentMemberId);

        if (memberParams.Gender != null)
        {
            query = query.Where(x => x.Gender == memberParams.Gender);
        }

        var minDob = DateOnly.FromDateTime(DateTime.Today.AddYears(-memberParams.MaxAge - 1));
        var maxDob = DateOnly.FromDateTime(DateTime.Today.AddYears(-memberParams.MinAge));

        query = query.Where(x => x.DateOfBirth >= minDob && x.DateOfBirth <= maxDob);

        if (memberParams.Distance.HasValue && memberParams.Distance > 0 && currentUserLocation != null)
        {
            var distanceInMeters = ConvertDistanceToMeters(memberParams.Distance.Value, memberParams.Unit);
            query = query.Where(m => m.Location != null && m.Location.IsWithinDistance(currentUserLocation, distanceInMeters));
        }

        query = memberParams.OrderBy switch
        {
            "created" => query.OrderByDescending(x => x.Created),
            _ => query.OrderByDescending(x => x.LastActive),
        };

        var result = await PaginationHelper.CreateAsync(query, memberParams.PageNumber, memberParams.PageSize);

        await cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(2));

        return result;
    }

    private static double ConvertDistanceToMeters(int distance, string unit)
    {
        return unit.ToLower() switch
        {
            "miles" => distance * 1609.34,
            _ => distance * 1000.0
        };
    }
}