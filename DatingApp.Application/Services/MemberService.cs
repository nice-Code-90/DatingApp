using DatingApp.Application.DTOs;
using DatingApp.Application.Helpers;
using DatingApp.Application.Interfaces;
using DatingApp.Domain.Entities;
using NetTopologySuite.Geometries;

namespace DatingApp.Application.Services;

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

    public async Task<PaginatedResult<MemberDto>> GetMembersWithFiltersAsync(MemberParams memberParams, Point? currentUserLocation)
    {
        var cacheKey = $"members:{memberParams.PageNumber}-{memberParams.PageSize}:{memberParams.Gender}:{memberParams.MinAge}-{memberParams.MaxAge}:{memberParams.OrderBy}:{memberParams.Distance}";
        
        var cachedResult = await cacheService.GetAsync<PaginatedResult<MemberDto>>(cacheKey);

        if (cachedResult != null) return cachedResult;

        var result = await uow.MemberRepository.GetMembersWithFiltersAsync(memberParams, currentUserLocation);

        await cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(2));

        return result;
    }

}