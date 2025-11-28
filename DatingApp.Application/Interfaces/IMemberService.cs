using DatingApp.Application.DTOs;
using DatingApp.Application.Helpers;
using NetTopologySuite.Geometries;

namespace DatingApp.Application.Interfaces;

public interface IMemberService
{
    Task<PaginatedResult<MemberDto>?> GetMembersWithFiltersAsync(MemberParams memberParams);
    Task<MemberDto?> GetMemberAsync(string id);
    Task<IReadOnlyList<PhotoDto>> GetMemberPhotosAsync(string memberId);
    Task<PhotoDto?> AddPhotoAsync(Stream photoStream, string fileName);
    Task<bool> UpdateMemberAsync(MemberUpdateDto memberUpdateDto);
    Task<bool> SetMainPhotoAsync(int photoId);
    Task<bool> DeletePhotoAsync(int photoId);
}