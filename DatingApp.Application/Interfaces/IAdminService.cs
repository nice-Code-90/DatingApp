using DatingApp.Application.DTOs;
using DatingApp.Application.Helpers;

namespace DatingApp.Application.Interfaces;

public interface IAdminService
{
    Task<IEnumerable<UserWithRolesDto>> GetUsersWithRolesAsync();
    Task<Result<IEnumerable<string>>> EditRolesAsync(string userId, string[] selectedRoles);
    Task<Result<object>> ApprovePhotoAsync(int photoId);
    Task<Result<object>> RejectPhotoAsync(int photoId);
    Task<IEnumerable<PhotoForApprovalDto>> GetPhotosForModerationAsync();

    Task ReindexAllMembersAsync();

    void StartSeedUsersProcess();
}