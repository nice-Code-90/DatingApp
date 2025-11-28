using DatingApp.Application.DTOs;
using DatingApp.Domain.Entities;

namespace DatingApp.Application.Interfaces;

public interface IUserRepository
{
    Task<IEnumerable<UserWithRolesDto>> GetUsersWithRolesAsync();
    Task<(bool Succeeded, string[]? Errors)> EditRolesAsync(string userId, string[] selectedRoles);
    Task<AppUser?> FindUserByIdAsync(string userId);
}