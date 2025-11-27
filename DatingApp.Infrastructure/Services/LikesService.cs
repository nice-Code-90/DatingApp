using DatingApp.Application.Helpers;
using DatingApp.Application.Interfaces;
using DatingApp.Domain.Entities;
using DatingApp.Infrastructure.Data;

namespace DatingApp.Infrastructure.Services;

public class LikesService(IUnitOfWork uow, ICacheService cacheService) : ILikesService
{
    public async Task<PaginatedResult<Member>> GetMemberLikesAsync(LikesParams likesParams)
    {
        var cacheKey = $"likes:{likesParams.MemberId}:{likesParams.Predicate}";
        var cachedResult = await cacheService.GetAsync<PaginatedResult<Member>>(cacheKey);

        if (cachedResult != null)
        {
            return cachedResult;
        }

        var query = uow.LikesRepository.GetLikesAsQueryable();
        IQueryable<Member> result;

        switch (likesParams.Predicate)
        {
            case "liked":
                result = query
                    .Where(like => like.SourceMemberId == likesParams.MemberId)
                    .Select(like => like.TargetMember);
                break;

            case "likedBy":
                result = query
                    .Where(like => like.TargetMemberId == likesParams.MemberId)
                    .Select(x => x.SourceMember);
                break;
            default: // "mutual"
                var likeIds = await uow.LikesRepository.GetCurrentMemberLikeIds(likesParams.MemberId);
                result = query
                    .Where(x => x.TargetMemberId == likesParams.MemberId && likeIds.Contains(x.SourceMemberId))
                    .Select(x => x.SourceMember);
                break;
        }

        var paginatedResult = await PaginationHelper.CreateAsync(result, likesParams.PageNumber, likesParams.PageSize);

        await cacheService.SetAsync(cacheKey, paginatedResult, TimeSpan.FromMinutes(5));

        return paginatedResult;
    }

    public async Task<bool> ToggleLikeAsync(string sourceMemberId, string targetMemberId)
    {
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