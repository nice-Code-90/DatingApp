using System;
using DatingApp.Domain.Entities;
using DatingApp.Application.Helpers;
using NetTopologySuite.Geometries;
using DatingApp.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.Infrastructure.Data;

public class MemberRepository(AppDbContext context) : IMemberRepository
{
    public async Task<Member?> GetMemberByIdAsync(string id)
    {
        return await context.Members.FindAsync(id);
    }

    public async Task<Member?> GetMemberForUpdate(string id)
    {
        return await context.Members
            .Include(x => x.User)
            .Include(x => x.Photos)
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(x => x.Id == id);
    }

    public IQueryable<Member> GetMembersAsQueryable()
    {
        return context.Members.AsQueryable();
    }

    public async Task<IReadOnlyList<Photo>> GetPhotosForMemberAsync(string memberId, bool isCurrentUser)
    {
        var query = context.Members
            .Where(x => x.Id == memberId)
            .SelectMany(x => x.Photos);

        if (isCurrentUser) query = query.IgnoreQueryFilters();

        return await query.ToListAsync();

    }

    public void Update(Member member)
    {
        context.Entry(member).State = EntityState.Modified;
    }
}
