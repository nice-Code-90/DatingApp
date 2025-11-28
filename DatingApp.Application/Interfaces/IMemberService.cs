using DatingApp.Application.DTOs;
using DatingApp.Application.Helpers;
using NetTopologySuite.Geometries;

namespace DatingApp.Application.Interfaces;

public interface IMemberService
{
    Task<PaginatedResult<MemberDto>> GetMembersWithFiltersAsync(MemberParams memberParams, Point? currentUserLocation);
    Task<bool> UpdateMemberAsync(string memberId, MemberUpdateDto memberUpdateDto);
    Task<bool> SetMainPhotoAsync(string memberId, int photoId);
}