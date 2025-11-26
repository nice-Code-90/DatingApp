using DatingApp.Application.DTOs;
using DatingApp.Application.Interfaces;

namespace DatingApp.Infrastructure.Services;

public class MemberService(IUnitOfWork uow, IGeocodingService geocodingService) : IMemberService
{
    public async Task<bool> SetMainPhotoAsync(string memberId, int photoId)
    {
        var member = await uow.MemberRepository.GetMemberForUpdate(memberId);
        if (member == null) return false;

        var photo = member.Photos.SingleOrDefault(x => x.Id == photoId);
        if (photo == null || photo.Url == member.ImageUrl) return false;

        member.ImageUrl = photo.Url;
        member.User.ImageUrl = photo.Url;

        return await uow.Complete();
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
        return await uow.Complete();
    }
}