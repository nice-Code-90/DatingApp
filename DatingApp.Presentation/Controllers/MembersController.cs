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
            var members = await memberService.GetMembersWithFiltersAsync(memberParams);

            if (members == null) return BadRequest("Your location is not available to filter by distance.");

            return Ok(members);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(MemberDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<MemberDto>> GetMember(string id)
        {
            var member = await memberService.GetMemberAsync(id);

            if (member == null) return NotFound();
            return Ok(member);

        }

        [HttpGet("{id}/photos")]
        public async Task<ActionResult<IReadOnlyList<PhotoDto>>> GetMemberPhotos(string id)
        {
            return Ok(await memberService.GetMemberPhotosAsync(id));
        }

        [HttpPut]
        public async Task<ActionResult> UpdateMember(MemberUpdateDto memberUpdateDto)
        {
            var result = await memberService.UpdateMemberAsync(memberUpdateDto);
            
            if (result) return NoContent();

            return BadRequest("Failed to update member");

        }

        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto( IFormFile file)
        {
            var result = await memberService.AddPhotoAsync(file.OpenReadStream(), file.FileName);

            if (result != null) return Ok(result);

            return BadRequest("Problem adding photo");
        }

        [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult> SetMainPhoto(int photoId)
        {
            var result = await memberService.SetMainPhotoAsync(photoId);
            
            if (result) return NoContent();

            return BadRequest("Problem setting main photo");
        }

        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            var result = await memberService.DeletePhotoAsync(photoId);
            
            if (result) return Ok();

            return BadRequest("Problem deleting the photo");
        }
    }
}
