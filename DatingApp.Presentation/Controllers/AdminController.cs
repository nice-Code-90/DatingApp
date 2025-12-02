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

        var (succeeded, errors) = await adminService.EditRolesAsync(userId, selectedRoles);

        if (!succeeded) return BadRequest(errors);

        return Ok();
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
        adminService.StartSeedUsersProcess();
        return Accepted("User seeding process has been started in the background.");
    }
}
