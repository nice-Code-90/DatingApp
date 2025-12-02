using DatingApp.Application.DTOs;
using DatingApp.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.Presentation.Controllers;

public class AdminController(
    IAdminService adminService
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

        var result = await adminService.EditRolesAsync(userId, selectedRoles);

        return HandleResult(result);
    }

    [Authorize(Policy = "ModeratePhotoRole")]
    [HttpGet("photos-to-moderate")]
    public async Task<ActionResult<IEnumerable<PhotoForApprovalDto>>> GetPhotosForModeration()
    {
        return Ok(await adminService.GetPhotosForModerationAsync());
    }


    [Authorize(Policy = "ModeratePhotoRole")]
    [HttpPost("approve-photo/{photoId}")]
    public async Task<ActionResult> ApprovePhoto(int photoId)
    {
        return HandleResult(await adminService.ApprovePhotoAsync(photoId));
    }


    [Authorize(Policy = "ModeratePhotoRole")]
    [HttpPost("reject-photo/{photoId}")]
    public async Task<ActionResult> RejectPhoto(int photoId)
    {
        return HandleResult(await adminService.RejectPhotoAsync(photoId));
    }

    [Authorize(Policy = "RequireAdminRole")]
    [HttpPost("seed-users")]
    public ActionResult SeedUsers()
    {
        adminService.StartSeedUsersProcess();
        return Accepted("User seeding process has been started in the background.");
    }
}
