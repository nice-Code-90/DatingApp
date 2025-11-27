using DatingApp.Application.DTOs;

namespace DatingApp.Application.Interfaces;

public interface IAdminService
{
    Task<IEnumerable<UserWithRolesDto>> GetUsersWithRolesAsync();
    Task<(bool Succeeded, string[]? Errors)> EditRolesAsync(string userId, string[] selectedRoles);
    Task<bool> ApprovePhotoAsync(int photoId);
    Task<bool> RejectPhotoAsync(int photoId);
}