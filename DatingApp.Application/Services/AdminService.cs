using DatingApp.Application.DTOs;
using DatingApp.Application.Interfaces;
using DatingApp.Application.Helpers;
using DatingApp.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DatingApp.Application.Services;

public class AdminService(
    IUserRepository userRepository,
    IUnitOfWork uow,
    IPhotoService photoService,
    ICacheService cacheService,
    IDataSeedingService dataSeedingService,
    UserManager<AppUser> userManager,
    IServiceProvider serviceProvider,
    ILogger<AdminService> logger) : IAdminService
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

    public async Task<Result<IEnumerable<string>>> EditRolesAsync(string userId, string[] selectedRoles)
    {
        var (succeeded, errors) = await userRepository.EditRolesAsync(userId, selectedRoles);

        if (succeeded)
        {
            await cacheService.RemoveAsync(UsersWithRolesCacheKey);
            var user = await userManager.FindByIdAsync(userId);
            if (user == null) return Result<IEnumerable<string>>.Failure("User not found");

            var roles = await userManager.GetRolesAsync(user);
            return Result<IEnumerable<string>>.Success(roles);
        }

        return Result<IEnumerable<string>>.Failure(string.Join(", ", errors ?? new[] { "Failed to edit roles" }));
    }

    public async Task<IEnumerable<PhotoForApprovalDto>> GetPhotosForModerationAsync()
    {
        return await uow.PhotoRepository.GetUnapprovedPhotos();
    }

    public void StartSeedUsersProcess()
    {
        dataSeedingService.StartSeedUsersProcess();
    }

    public async Task<Result<object>> ApprovePhotoAsync(int photoId)
    {
        var photo = await uow.PhotoRepository.GetPhotoById(photoId);
        if (photo == null) return Result.Failure("Photo not found");

        var member = await uow.MemberRepository.GetMemberForUpdate(photo.MemberId);
        if (member == null) return Result.Failure("Member not found");

        photo.IsApproved = true;

        if (member.ImageUrl == null)
        {
            member.ImageUrl = photo.Url;
            member.User.ImageUrl = photo.Url;
        }

        if (await uow.Complete())
        {
            await cacheService.RemoveByPrefixAsync("members:");
            return Result<object>.Success(new { });
        }

        return Result.Failure("Failed to approve photo");
    }

    public async Task<Result<object>> RejectPhotoAsync(int photoId)
    {
        var photo = await uow.PhotoRepository.GetPhotoById(photoId);
        if (photo == null) return Result.Failure("Photo not found");

        if (photo.PublicId != null)
        {
            if (!await photoService.DeletePhotoAsync(photo.PublicId)) return Result.Failure("Failed to delete photo from Cloudinary");
        }

        uow.PhotoRepository.RemovePhoto(photo);
        return await uow.Complete() ? Result<object>.Success(new { }) : Result.Failure("Failed to reject photo");
    }

    public async Task ReindexAllMembersAsync()
    {
        logger.LogInformation("Starting Qdrant re-indexing process...");
        try
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var scopedUow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var scopedAiService = scope.ServiceProvider.GetRequiredService<IAiMatchmakingService>();

                var allMembers = await scopedUow.MemberRepository.GetAllMembersAsync();
                logger.LogInformation("Found {Count} members to re-index.", allMembers.Count());

                foreach (var member in allMembers)
                {
                    await scopedAiService.UpdateMemberProfileAsync(member);
                }
                logger.LogInformation("Successfully re-indexed all members in Qdrant.");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred during Qdrant re-indexing.");
        }
    }
}