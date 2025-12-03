using DatingApp.Application.DTOs;
using DatingApp.Application.Helpers;
using DatingApp.Application.Interfaces;
using DatingApp.Domain.Entities;
using NetTopologySuite.Geometries;

namespace DatingApp.Application.Services;

public class MemberService(IUnitOfWork uow, IGeocodingService geocodingService, ICacheService cacheService, IPhotoService photoService, ICurrentUserService currentUserService, IAiMatchmakingService aiMatchmakingService) : IMemberService
{
    public async Task<Result<PhotoDto>> AddPhotoAsync(Stream photoStream, string fileName)
    {
        var memberId = currentUserService.MemberId;
        if (string.IsNullOrEmpty(memberId)) return Result<PhotoDto>.Failure("User not found");

        var member = await uow.MemberRepository.GetMemberForUpdate(memberId);
        if (member == null) return Result<PhotoDto>.Failure("Member not found");

        var result = await photoService.UploadPhotoAsync(photoStream, fileName);
        if (result == null) return Result<PhotoDto>.Failure("Failed to upload photo");

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
            var photoDto = new PhotoDto { Id = photo.Id, Url = photo.Url, IsApproved = photo.IsApproved, IsMain = false };
            return Result<PhotoDto>.Success(photoDto);
        }

        return Result<PhotoDto>.Failure("Problem adding photo");
    }

    public async Task<Result<MemberDto>> GetMemberAsync(string id)
    {
        var member = await uow.MemberRepository.GetMemberDtoByIdAsync(id);
        return member != null ? Result<MemberDto>.Success(member) : Result<MemberDto>.Failure("Member not found");
    }

    public async Task<Result<IReadOnlyList<PhotoDto>>> GetMemberPhotosAsync(string memberId)
    {
        var isCurrentUser = currentUserService.MemberId == memberId;
        var photos = await uow.MemberRepository.GetPhotosForMemberAsync(memberId, isCurrentUser);
        return Result<IReadOnlyList<PhotoDto>>.Success(photos);
    }

    public async Task<Result<object>> DeletePhotoAsync(int photoId)
    {
        var memberId = currentUserService.MemberId;
        if (string.IsNullOrEmpty(memberId)) return Result.Failure("User not found");

        var success = await photoService.DeleteMemberPhotoAsync(memberId, photoId);
        return success ? Result<object>.Success(new { }) : Result.Failure("Problem deleting the photo");
    }

    public async Task<Result<object>> SetMainPhotoAsync(int photoId)
    {
        var memberId = currentUserService.MemberId;
        if (string.IsNullOrEmpty(memberId)) return Result.Failure("User not found");

        var member = await uow.MemberRepository.GetMemberForUpdate(memberId);
        if (member == null) return Result.Failure("Member not found");

        var photo = member.Photos.SingleOrDefault(x => x.Id == photoId);
        if (photo == null) return Result.Failure("Photo not found");
        if (photo.Url == member.ImageUrl) return Result.Failure("This is already your main photo");

        member.ImageUrl = photo.Url;
        member.User.ImageUrl = photo.Url;

        var result = await uow.Complete();
        if (result) await cacheService.RemoveByPrefixAsync("members:");

        return result ? Result<object>.Success(new { }) : Result.Failure("Problem setting main photo");
    }

    public async Task<Result<object>> UpdateMemberAsync(MemberUpdateDto memberUpdateDto)
    {
        var memberId = currentUserService.MemberId;
        if (string.IsNullOrEmpty(memberId)) return Result.Failure("User not found");

        var member = await uow.MemberRepository.GetMemberForUpdate(memberId);
        if (member == null) return Result.Failure("Member not found");

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
        if (result)
        {
            await cacheService.RemoveByPrefixAsync("members:");
            await aiMatchmakingService.UpdateMemberProfileAsync(member);
        }

        return result ? Result<object>.Success(new { }) : Result.Failure("Failed to update member");
    }

    public async Task<Result<PaginatedResult<MemberDto>>> GetMembersWithFiltersAsync(MemberParams memberParams)
    {
        var memberId = currentUserService.MemberId;
        if (string.IsNullOrEmpty(memberId)) return Result<PaginatedResult<MemberDto>>.Failure("User not found");
        memberParams.CurrentMemberId = memberId;
        var cacheKey = $"members:{memberParams.PageNumber}-{memberParams.PageSize}:{memberParams.Gender}:{memberParams.MinAge}-{memberParams.MaxAge}:{memberParams.OrderBy}:{memberParams.Distance}";

        var cachedResult = await cacheService.GetAsync<PaginatedResult<MemberDto>>(cacheKey);
        if (cachedResult != null) return Result<PaginatedResult<MemberDto>>.Success(cachedResult);

        Point? userLocation = null;
        if (memberParams.Distance.HasValue && memberParams.Distance > 0)
        {

            var currentUser = await uow.MemberRepository.GetMemberByIdAsync(memberParams.CurrentMemberId);
            if (currentUser?.Location == null) return Result<PaginatedResult<MemberDto>>.Failure("Your location is not available to filter by distance.");
            userLocation = currentUser.Location;
        }

        var result = await uow.MemberRepository.GetMembersWithFiltersAsync(memberParams, userLocation);

        await cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(2));

        return Result<PaginatedResult<MemberDto>>.Success(result);
    }

}