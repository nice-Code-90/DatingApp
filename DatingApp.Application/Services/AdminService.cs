using DatingApp.Application.DTOs;
using DatingApp.Application.Interfaces;

namespace DatingApp.Application.Services;

public class AdminService(
    IUserRepository userRepository,
    IUnitOfWork uow,
    IPhotoService photoService,
    ICacheService cacheService) : IAdminService
{
    private const string UsersWithRolesCacheKey = "users-with-roles";

    public async Task<IEnumerable<UserWithRolesDto>> GetUsersWithRolesAsync()
    {
        var cachedUsers = await cacheService.GetAsync<IEnumerable<UserWithRolesDto>>(UsersWithRolesCacheKey);
        if (cachedUsers != null) return cachedUsers;

        var users = await userRepository.GetUsersWithRolesAsync();

        await cacheService.SetAsync(UsersWithRolesCacheKey, users, TimeSpan.FromMinutes(15));

        return users;
    }

    public async Task<(bool Succeeded, string[]? Errors)> EditRolesAsync(string userId, string[] selectedRoles)
    {
        var (succeeded, errors) = await userRepository.EditRolesAsync(userId, selectedRoles);

        if (succeeded)
        {
            await cacheService.RemoveAsync(UsersWithRolesCacheKey);
        }

        return (succeeded, errors);
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