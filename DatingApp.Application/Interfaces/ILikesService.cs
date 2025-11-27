using DatingApp.Application.Helpers;
using DatingApp.Domain.Entities;

namespace DatingApp.Application.Interfaces;

public interface ILikesService
{
    Task<PaginatedResult<Member>> GetMemberLikesAsync(LikesParams likesParams);
    Task<bool> ToggleLikeAsync(string sourceMemberId, string targetMemberId);
}