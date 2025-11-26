using DatingApp.Application.DTOs;

namespace DatingApp.Application.Interfaces;

public interface IAdminService
{
    Task<IEnumerable<UserWithRolesDto>> GetUsersWithRolesAsync();
}