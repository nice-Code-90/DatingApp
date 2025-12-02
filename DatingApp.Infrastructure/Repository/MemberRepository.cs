using DatingApp.Domain.Entities;
using DatingApp.Application.DTOs;
using DatingApp.Application.Extensions;
using DatingApp.Application.Helpers;
using NetTopologySuite.Geometries;
using DatingApp.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using DatingApp.Infrastructure.Data;
using DatingApp.Infrastructure.Helpers;

namespace DatingApp.Infrastructure.Repository;

public class MemberRepository(AppDbContext context) : IMemberRepository
{
    public async Task<Member?> GetMemberByIdAsync(string id)
    {
        return await context.Members.FindAsync(id);
    }

    public async Task<MemberDto?> GetMemberDtoByIdAsync(string id)
    {
        return await context.Members
            .Where(m => m.Id == id)
            .Select(MemberExtensions.ToDtoProjection())
            .SingleOrDefaultAsync();
    }

    public async Task<Member?> GetMemberForUpdate(string id)
    {
        return await context.Members
            .Include(x => x.User)
            .Include(x => x.Photos)
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(x => x.Id == id);
    }

    public async Task<PaginatedResult<MemberDto>> GetMembersWithFiltersAsync(MemberParams memberParams, Point? currentUserLocation)
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

        var dtoQuery = query.Select(MemberExtensions.ToDtoProjection());

        return await PaginationHelper.CreateAsync(dtoQuery, memberParams.PageNumber, memberParams.PageSize);
    }

    private static double ConvertDistanceToMeters(int distance, string unit)
    {
        return unit.ToLower() switch
        {
            "miles" => distance * 1609.34,
            _ => distance * 1000.0
        };
    }

    public async Task<IReadOnlyList<PhotoDto>> GetPhotosForMemberAsync(string memberId, bool isCurrentUser)
    {
        var query = context.Members
            .Where(x => x.Id == memberId)
            .SelectMany(x => x.Photos);

        if (isCurrentUser) query = query.IgnoreQueryFilters();

        return await query
            .Select(PhotoExtensions.ToDtoProjection())
            .ToListAsync();

    }

    public void Update(Member member)
    {
        context.Entry(member).State = EntityState.Modified;
    }

    public async Task<IEnumerable<Member>> GetMembersForAiSyncAsync()
    {
        return await context.Members
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<Member>> GetMembersByIdsAsync(IEnumerable<string> ids)
    {
        return await context.Members
            .Include(m => m.Photos)
            .Where(m => ids.Contains(m.Id))
            .ToListAsync();
    }
}
