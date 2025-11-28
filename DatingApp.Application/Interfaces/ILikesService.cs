using DatingApp.Application.DTOs;
using DatingApp.Application.Helpers;

namespace DatingApp.Application.Interfaces;

public interface ILikesService
{
    Task<PaginatedResult<MemberDto>> GetMemberLikesAsync(LikesParams likesParams);
    Task<bool> ToggleLikeAsync(string sourceMemberId, string targetMemberId);
}