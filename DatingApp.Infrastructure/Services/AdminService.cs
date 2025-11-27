using DatingApp.Application.DTOs;
using DatingApp.Application.Interfaces;
using DatingApp.Domain.Entities;
using DatingApp.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.Infrastructure.Services;

public class AdminService(UserManager<AppUser> userManager, AppDbContext context, ICacheService cacheService, IUnitOfWork uow, IPhotoService photoService) : IAdminService
{
    private const string UsersWithRolesCacheKey = "users-with-roles";

    public async Task<IEnumerable<UserWithRolesDto>> GetUsersWithRolesAsync()
    {
        var cachedUsers = await cacheService.GetAsync<IEnumerable<UserWithRolesDto>>(UsersWithRolesCacheKey);
        if (cachedUsers != null) return cachedUsers;

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

        await cacheService.SetAsync(UsersWithRolesCacheKey, users, TimeSpan.FromMinutes(15));

        return users;
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

        await cacheService.RemoveAsync(UsersWithRolesCacheKey);

        return (true, null);
    }

    public async Task<bool> ApprovePhotoAsync(int photoId)
    {
        var photo = await uow.PhotoRepository.GetPhotoById(photoId);
        if (photo == null) return false;

        var member = await uow.MemberRepository.GetMemberForUpdate(photo.MemberId);
        if (member == null) return false;

        photo.IsApproved = true;

        if (member.ImageUrl == null)
        {
            member.ImageUrl = photo.Url;
            member.User.ImageUrl = photo.Url;
        }

        if (await uow.Complete())
        {
            await cacheService.RemoveByPrefixAsync("members:");
            return true;
        }

        return false;
    }

    public async Task<bool> RejectPhotoAsync(int photoId)
    {
        var photo = await uow.PhotoRepository.GetPhotoById(photoId);
        if (photo == null) return false;

        if (photo.PublicId != null)
        {
            if (!await photoService.DeletePhotoAsync(photo.PublicId)) return false;
        }

        uow.PhotoRepository.RemovePhoto(photo);
        return await uow.Complete();
    }
}