using DatingApp.Application.DTOs;

namespace DatingApp.Application.Interfaces;

public interface IMemberService
{
    Task<bool> UpdateMemberAsync(string memberId, MemberUpdateDto memberUpdateDto);
    Task<bool> SetMainPhotoAsync(string memberId, int photoId);
}