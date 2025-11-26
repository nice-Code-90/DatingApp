using DatingApp.Application.DTOs;
using DatingApp.Application.Interfaces;
using DatingApp.Domain.Entities;
using DatingApp.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.Infrastructure.Services;

public class AdminService(UserManager<AppUser> userManager, AppDbContext context) : IAdminService
{
    public async Task<IEnumerable<UserWithRolesDto>> GetUsersWithRolesAsync()
    {
        var users = await context.Users
            .OrderBy(u => u.Email)
            .Select(u => new UserWithRolesDto
            {
                Id = u.Id,
                Email = u.Email,
                Roles = (from userRole in context.UserRoles
                         where userRole.UserId == u.Id
                         join role in context.Roles
                         on userRole.RoleId equals role.Id
                         select role.Name
                        ).ToList()
            })
            .ToListAsync();

        return users;
    }
}