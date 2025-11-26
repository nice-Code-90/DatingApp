using DatingApp.Application.Helpers;
using DatingApp.Application.Interfaces;
using DatingApp.Domain.Entities;
using DatingApp.Infrastructure.Data;

namespace DatingApp.Infrastructure.Services;

public class LikesService(IUnitOfWork uow) : ILikesService
{
    public async Task<PaginatedResult<Member>> GetMemberLikesAsync(LikesParams likesParams)
    {
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

        return await PaginationHelper.CreateAsync(result, likesParams.PageNumber, likesParams.PageSize);
    }
}