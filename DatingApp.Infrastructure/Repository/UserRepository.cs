using DatingApp.Application.DTOs;
using DatingApp.Application.Interfaces;
using DatingApp.Domain.Entities;
using DatingApp.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.Infrastructure.Repository;

public class UserRepository(UserManager<AppUser> userManager, AppDbContext context) : IUserRepository
{
    public async Task<IEnumerable<UserWithRolesDto>> GetUsersWithRolesAsync()
    {
        return await context.Users
            .OrderBy(u => u.Email)
            .Select(u => new UserWithRolesDto
            {
                Id = u.Id,
                Email = u.Email ?? string.Empty,
                Roles = (from userRole in context.UserRoles
                         where userRole.UserId == u.Id
                         join role in context.Roles
                         on userRole.RoleId equals role.Id
                         select role.Name
                        ).ToList()
            })
            .ToListAsync();
    }

    public async Task<(bool Succeeded, string[]? Errors)> EditRolesAsync(string userId, string[] selectedRoles)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user == null) return (false, new[] { "Could not retrieve user" });

        var userRoles = await userManager.GetRolesAsync(user);

        var result = await userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles));
        if (!result.Succeeded) return (false, result.Errors.Select(e => e.Description).ToArray());

        result = await userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));
        if (!result.Succeeded) return (false, result.Errors.Select(e => e.Description).ToArray());

        return (true, null);
    }

    public async Task<AppUser?> FindUserByIdAsync(string userId)
    {
        return await userManager.FindByIdAsync(userId);
    }

    public async Task<IEnumerable<Member>> GetMembersForAiSyncAsync()
    {
        return await context.Members
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Member?> GetMemberByIdAsync(string id)
    {
        return await context.Members
            .Include(m => m.Photos)
            .FirstOrDefaultAsync(m => m.Id == id);
    }
}