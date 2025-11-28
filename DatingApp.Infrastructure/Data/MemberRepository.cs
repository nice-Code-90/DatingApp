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

    public async Task<PaginatedResult<Member>> GetMembersWithFiltersAsync(MemberParams memberParams, Point? currentUserLocation)
    {
        var query = context.Members.AsQueryable();

        query = query.Where(x => x.Id != memberParams.CurrentMemberId);

        if (memberParams.Gender != null)
        {
            query = query.Where(x => x.Gender == memberParams.Gender);
        }

        var minDob = DateOnly.FromDateTime(DateTime.Today.AddYears(-memberParams.MaxAge - 1));
        var maxDob = DateOnly.FromDateTime(DateTime.Today.AddYears(-memberParams.MinAge));

        query = query.Where(x => x.DateOfBirth >= minDob && x.DateOfBirth <= maxDob);

        if (memberParams.Distance.HasValue && memberParams.Distance > 0 && currentUserLocation != null)
        {
            var distanceInMeters = ConvertDistanceToMeters(memberParams.Distance.Value, memberParams.Unit);
            query = query.Where(m => m.Location != null && m.Location.IsWithinDistance(currentUserLocation, distanceInMeters));
        }

        query = memberParams.OrderBy switch
        {
            "created" => query.OrderByDescending(x => x.Created),
            _ => query.OrderByDescending(x => x.LastActive),
        };

        return await PaginationHelper.CreateAsync(query, memberParams.PageNumber, memberParams.PageSize);
    }

    private static double ConvertDistanceToMeters(int distance, string unit)
    {
        return unit.ToLower() switch
        {
            "miles" => distance * 1609.34,
            _ => distance * 1000.0
        };
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
