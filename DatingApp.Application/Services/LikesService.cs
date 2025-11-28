using DatingApp.Application.DTOs;
using DatingApp.Application.Helpers;
using DatingApp.Application.Interfaces;
using DatingApp.Domain.Entities;

namespace DatingApp.Application.Services;

public class LikesService(IUnitOfWork uow, ICacheService cacheService, ICurrentUserService currentUserService) : ILikesService
{
    public async Task<PaginatedResult<MemberDto>> GetMemberLikesAsync(LikesParams likesParams)
    {
        var cacheKey = $"likes:{likesParams.MemberId}:{likesParams.Predicate}";
        var cachedResult = await cacheService.GetAsync<PaginatedResult<MemberDto>>(cacheKey);

        if (cachedResult != null)
        {
            return cachedResult;
        }

        var paginatedResult = await uow.LikesRepository.GetMemberLikesAsync(likesParams);

        await cacheService.SetAsync(cacheKey, paginatedResult, TimeSpan.FromMinutes(5));

        return paginatedResult;
    }

    public async Task<IReadOnlyList<string>> GetCurrentMemberLikeIds()
    {
        var memberId = currentUserService.MemberId;
        if (string.IsNullOrEmpty(memberId)) return new List<string>();
        return await uow.LikesRepository.GetCurrentMemberLikeIds(memberId);
    }

    public async Task<bool> ToggleLikeAsync(string targetMemberId)
    {
        var sourceMemberId = currentUserService.MemberId;
        if (string.IsNullOrEmpty(sourceMemberId)) return false;

        if (sourceMemberId == targetMemberId) return false; 

        var existingLike = await uow.LikesRepository.GetMemberLike(sourceMemberId, targetMemberId);

        if (existingLike == null)
        {
            var like = new MemberLike
            {
                SourceMemberId = sourceMemberId,
                TargetMemberId = targetMemberId
            };
            uow.LikesRepository.AddLike(like);
        }
        else
        {
            uow.LikesRepository.DeleteLike(existingLike);
        }

        var result = await uow.Complete();

        if (!result) return false;

        await cacheService.RemoveByPrefixAsync($"likes:{sourceMemberId}");
        await cacheService.RemoveByPrefixAsync($"likes:{targetMemberId}");

        return true;
    }
}