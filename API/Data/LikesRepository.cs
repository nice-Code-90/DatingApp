using System;
using API.Entities;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace API.Data;

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

    public async Task<IReadOnlyList<Member>> GetMemberLikes(string predicate, string memberId)
    {
        var query = context.Likes.AsQueryable();

        switch (predicate)
        {
            case "liked":
                return await query
                    .Where(x => x.SourceMemberId == memberId)
                    .Select(x => x.TargetMember)
                    .ToListAsync();

            case "likedBy":
                return await query
                    .Where(x => x.TargetMemberId == memberId)
                    .Select(x => x.SourceMember)
                    .ToListAsync();
            default:
                var likeIds = await GetCurrentMemberLikeIds(memberId);

                return await query
                    .Where(x => x.TargetMemberId == memberId
                        && likeIds.Contains(x.SourceMemberId))
                    .Select(x => x.SourceMember)
                    .ToListAsync();
        }
    }

    public async Task<bool> SaveAllChanges()
    {
        return await context.SaveChangesAsync() > 0;
    }
}
