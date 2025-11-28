using System;
using DatingApp.Domain.Entities;
using DatingApp.Application.Helpers;
using DatingApp.Application.DTOs;
using DatingApp.Application.Extensions;
using DatingApp.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.Infrastructure.Data;

public class LikesRepository(AppDbContext context) : ILikesRepository
{
    public void AddLike(MemberLike like)
    {
        context.Likes.Add(like);
    }

    public void DeleteLike(MemberLike like)
    {
        context.Likes.Remove(like);
    }

    public async Task<IReadOnlyList<string>> GetCurrentMemberLikeIds(string memberId)
    {
        return await context.Likes
            .Where(x => x.SourceMemberId == memberId)
            .Select(x => x.TargetMemberId)
            .ToListAsync();
    }

    public async Task<MemberLike?> GetMemberLike(string sourceMemberId, string targetMemberId)
    {
        return await context.Likes.FindAsync(sourceMemberId, targetMemberId);
    }

    public async Task<PaginatedResult<MemberDto>> GetMemberLikesAsync(LikesParams likesParams)
    {
        var query = context.Likes.AsQueryable();
        IQueryable<Member> membersQuery;

        switch (likesParams.Predicate)
        {
            case "liked":
                membersQuery = query
                    .Where(like => like.SourceMemberId == likesParams.MemberId)
                    .Select(like => like.TargetMember);
                break;

            case "likedBy":
                membersQuery = query
                    .Where(like => like.TargetMemberId == likesParams.MemberId)
                    .Select(x => x.SourceMember);
                break;
            default: // "mutual"
                var likeIds = await GetCurrentMemberLikeIds(likesParams.MemberId);
                membersQuery = query
                    .Where(x => x.TargetMemberId == likesParams.MemberId && likeIds.Contains(x.SourceMemberId))
                    .Select(x => x.SourceMember);
                break;
        }

        var dtoQuery = membersQuery.Select(MemberExtensions.ToDtoProjection());

        return await PaginationHelper.CreateAsync(dtoQuery, likesParams.PageNumber, likesParams.PageSize);
    }
}
