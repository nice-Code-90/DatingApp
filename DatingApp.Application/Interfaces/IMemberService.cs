using DatingApp.Application.DTOs;
using DatingApp.Application.Helpers;
using NetTopologySuite.Geometries;

namespace DatingApp.Application.Interfaces;

public interface IMemberService
{
    Task<Result<PaginatedResult<MemberDto>>> GetMembersWithFiltersAsync(MemberParams memberParams);
    Task<Result<MemberDto>> GetMemberAsync(string id);
    Task<Result<IReadOnlyList<PhotoDto>>> GetMemberPhotosAsync(string memberId);
    Task<Result<PhotoDto>> AddPhotoAsync(Stream photoStream, string fileName);
    Task<Result<object>> UpdateMemberAsync(MemberUpdateDto memberUpdateDto);
    Task<Result<object>> SetMainPhotoAsync(int photoId);
    Task<Result<object>> DeletePhotoAsync(int photoId);
}