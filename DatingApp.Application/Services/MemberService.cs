using DatingApp.Application.DTOs;
using DatingApp.Application.Helpers;
using DatingApp.Application.Interfaces;
using DatingApp.Domain.Entities;
using NetTopologySuite.Geometries;

namespace DatingApp.Application.Services;

public class MemberService(IUnitOfWork uow, IGeocodingService geocodingService, ICacheService cacheService, IPhotoService photoService, ICurrentUserService currentUserService) : IMemberService
{
    public async Task<PhotoDto?> AddPhotoAsync(Stream photoStream, string fileName)
    {
        var memberId = currentUserService.MemberId;
        if (string.IsNullOrEmpty(memberId)) return null;

        var member = await uow.MemberRepository.GetMemberForUpdate(memberId);
        if (member == null) return null;

        var result = await photoService.UploadPhotoAsync(photoStream, fileName);
        if (result == null) return null;

        var photo = new Photo
        {
            Url = result.Url,
            PublicId = result.PublicId,
            MemberId = memberId,
            IsApproved = false
        };

        member.Photos.Add(photo);

        if (await uow.Complete())
        {
            return new PhotoDto { Id = photo.Id, Url = photo.Url, IsApproved = photo.IsApproved, IsMain = false };
        }

        return null;
    }

    public async Task<MemberDto?> GetMemberAsync(string id)
    {
        return await uow.MemberRepository.GetMemberDtoByIdAsync(id);
    }

    public async Task<IReadOnlyList<PhotoDto>> GetMemberPhotosAsync(string memberId)
    {
        var isCurrentUser = currentUserService.MemberId == memberId;
        return await uow.MemberRepository.GetPhotosForMemberAsync(memberId, isCurrentUser);
    }

    public async Task<bool> DeletePhotoAsync(int photoId)
    {
        var memberId = currentUserService.MemberId;
        if (string.IsNullOrEmpty(memberId)) return false;

        return await photoService.DeleteMemberPhotoAsync(memberId, photoId);
    }

    public async Task<bool> SetMainPhotoAsync(int photoId)
    {
        var memberId = currentUserService.MemberId;
        if (string.IsNullOrEmpty(memberId)) return false;

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

    public async Task<bool> UpdateMemberAsync(MemberUpdateDto memberUpdateDto)
    {
        var memberId = currentUserService.MemberId;
        if (string.IsNullOrEmpty(memberId)) return false;

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

    public async Task<PaginatedResult<MemberDto>?> GetMembersWithFiltersAsync(MemberParams memberParams)
    {
        var memberId = currentUserService.MemberId;
        if (string.IsNullOrEmpty(memberId)) return null;
        memberParams.CurrentMemberId = memberId;
        var cacheKey = $"members:{memberParams.PageNumber}-{memberParams.PageSize}:{memberParams.Gender}:{memberParams.MinAge}-{memberParams.MaxAge}:{memberParams.OrderBy}:{memberParams.Distance}";
        
        var cachedResult = await cacheService.GetAsync<PaginatedResult<MemberDto>>(cacheKey);
        if (cachedResult != null) return cachedResult;

        Point? userLocation = null;
        if (memberParams.Distance.HasValue && memberParams.Distance > 0)
        {
           
            var currentUser = await uow.MemberRepository.GetMemberByIdAsync(memberParams.CurrentMemberId);
            if (currentUser?.Location == null) return null;
            userLocation = currentUser.Location;
        }

        var result = await uow.MemberRepository.GetMembersWithFiltersAsync(memberParams, userLocation);

        await cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(2));

        return result;
    }

}