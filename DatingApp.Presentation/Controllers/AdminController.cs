using DatingApp.Application.DTOs;
using DatingApp.Application.Interfaces;
using DatingApp.Domain.Entities;
using DatingApp.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.Presentation.Controllers;

public class AdminController(
    UserManager<AppUser> userManager,
    IUnitOfWork uow,
    IAdminService adminService,
    IGeocodingService geocodingService,
    IAiMatchmakingService aiMatchmakingService,
    ILoggerFactory loggerFactory,
    IServiceProvider serviceProvider
    ) : BaseApiController
{
    [Authorize(Policy = "RequireAdminRole")]
    [HttpGet("users-with-roles")]
    public async Task<ActionResult<IEnumerable<UserWithRolesDto>>> GetUsersWithRoles()
    {
        return Ok(await adminService.GetUsersWithRolesAsync());
    }

    [Authorize(Policy = "RequireAdminRole")]
    [HttpPost("edit-roles/{userId}")]
    public async Task<ActionResult<IList<string>>> EditRoles(string userId, [FromQuery] string roles)
    {
        if (string.IsNullOrEmpty(roles)) return BadRequest("You must select at least one role");

        var selectedRoles = roles.Split(",").ToArray();

        var (succeeded, errors) = await adminService.EditRolesAsync(userId, selectedRoles);

        if (!succeeded) return BadRequest(errors);

        var user = await userManager.FindByIdAsync(userId);
        if (user is null) return NotFound("User not found");

        return Ok(await userManager.GetRolesAsync(user));

    }

    [Authorize(Policy = "ModeratePhotoRole")]
    [HttpGet("photos-to-moderate")]
    public async Task<ActionResult<IEnumerable<PhotoForApprovalDto>>> GetPhotosForModeration()
    {
        return Ok(await uow.PhotoRepository.GetUnapprovedPhotos());
    }


    [Authorize(Policy = "ModeratePhotoRole")]
    [HttpPost("approve-photo/{photoId}")]
    public async Task<ActionResult> ApprovePhoto(int photoId)
    {
        var result = await adminService.ApprovePhotoAsync(photoId);

        if (result) return Ok();

        return BadRequest("Failed to approve photo");
    }


    [Authorize(Policy = "ModeratePhotoRole")]
    [HttpPost("reject-photo/{photoId}")]
    public async Task<ActionResult> RejectPhoto(int photoId)
    {
        var result = await adminService.RejectPhotoAsync(photoId);

        if (result) return Ok();

        return BadRequest("Failed to reject photo.");
    }

    [Authorize(Policy = "RequireAdminRole")]
    [HttpPost("seed-users")]
    public ActionResult SeedUsers()
    {

        _ = Task.Run(async () =>
        {
            using var scope = serviceProvider.CreateScope();
            var scopedUserManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
            var scopedGeocodingService = scope.ServiceProvider.GetRequiredService<IGeocodingService>();
            var scopedAiMatchmakingService = scope.ServiceProvider.GetRequiredService<IAiMatchmakingService>();
            var seedLogger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DatingApp.Infrastructure.Data.Seed");

            await Seed.SeedUsers(seedLogger, scopedUserManager, scopedGeocodingService, scopedAiMatchmakingService);
        });

        return Accepted("User seeding process has been started in the background.");
    }
}
