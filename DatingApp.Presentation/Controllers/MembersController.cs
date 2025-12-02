using DatingApp.Application.DTOs;
using DatingApp.Application.Extensions;
using DatingApp.Application.Helpers;
using DatingApp.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.Presentation.Controllers
{
    [Authorize]
    public class MembersController(IMemberService memberService) : BaseApiController
    {
        [ProducesResponseType(typeof(PaginatedResult<MemberDto>), StatusCodes.Status200OK)]
        [HttpGet]
        public async Task<ActionResult<PaginatedResult<MemberDto>>> GetMembers(
            [FromQuery] MemberParams memberParams)
        {
            return HandleResult(await memberService.GetMembersWithFiltersAsync(memberParams));
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(MemberDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<MemberDto>> GetMember(string id)
        {
            return HandleResult(await memberService.GetMemberAsync(id));
        }

        [HttpGet("{id}/photos")]
        public async Task<ActionResult<IReadOnlyList<PhotoDto>>> GetMemberPhotos(string id)
        {
            return HandleResult(await memberService.GetMemberPhotosAsync(id));
        }

        [HttpPut]
        public async Task<ActionResult> UpdateMember(MemberUpdateDto memberUpdateDto)
        {
            return HandleResult(await memberService.UpdateMemberAsync(memberUpdateDto));
        }

        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
        {
            return HandleResult(await memberService.AddPhotoAsync(file.OpenReadStream(), file.FileName));
        }

        [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult> SetMainPhoto(int photoId)
        {
            return HandleResult(await memberService.SetMainPhotoAsync(photoId));
        }

        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            return HandleResult(await memberService.DeletePhotoAsync(photoId));
        }
    }
}
