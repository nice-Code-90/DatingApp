using DatingApp.Domain.Entities;
using DatingApp.Application.DTOs;
using DatingApp.Application.Extensions;
using DatingApp.Application.Helpers;
using DatingApp.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.Presentation.Controllers;

public class LikesController(ILikesService likesService) : BaseApiController
{
    [HttpPost("{targetMemberId}")]
    public async Task<ActionResult> ToggleLike(string targetMemberId)
    {
        var result = await likesService.ToggleLikeAsync(targetMemberId);

        if (result) return Ok();

        return BadRequest("Failed to update like");
    }

    [HttpGet("list")]
    public async Task<ActionResult<IReadOnlyList<string>>> GetCurrentMemberLikeIds()
    {
        return Ok(await likesService.GetCurrentMemberLikeIds());
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedResult<MemberDto>>> GetMemberLikes(
        [FromQuery] LikesParams likesParams)
    {
        return Ok(await likesService.GetMemberLikesAsync(likesParams));
    }
}
