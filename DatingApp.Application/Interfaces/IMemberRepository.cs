using System;
using DatingApp.Domain.Entities;
using DatingApp.Application.DTOs;
using DatingApp.Application.Helpers;
using NetTopologySuite.Geometries;

namespace DatingApp.Application.Interfaces;

public interface IMemberRepository
{
    void Update(Member member);
    Task<Member?> GetMemberByIdAsync(string id);
    Task<MemberDto?> GetMemberDtoByIdAsync(string id);
    Task<PaginatedResult<MemberDto>> GetMembersWithFiltersAsync(MemberParams memberParams, Point? currentUserLocation);
    Task<IReadOnlyList<PhotoDto>> GetPhotosForMemberAsync(string memberId, bool isCurrentUser);
    Task<Member?> GetMemberForUpdate(string id);
}