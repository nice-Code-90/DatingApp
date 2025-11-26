using System;
using DatingApp.Domain.Entities;
using DatingApp.Application.Helpers;
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

    public IQueryable<MemberLike> GetLikesAsQueryable()
    {
        return context.Likes.AsQueryable();
    }
}
