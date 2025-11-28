using DatingApp.Application.DTOs;
using DatingApp.Domain.Entities;
using DatingApp.Application.Extensions;
using DatingApp.Application.Helpers;
using DatingApp.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.Presentation.Controllers
{
    [Authorize]
    public class MembersController(IUnitOfWork uow,
    IPhotoService photoService, IMemberService memberService) : BaseApiController
    {
        [ProducesResponseType(typeof(PaginatedResult<MemberDto>), StatusCodes.Status200OK)]
        [HttpGet]
        public async Task<ActionResult<PaginatedResult<MemberDto>>> GetMembers(
            [FromQuery] MemberParams memberParams)
        {
            if (memberParams.Distance > 0)
            {
                var currentUserId = User.GetMemberId();
                memberParams.CurrentMemberId = currentUserId;
                var currentUser = await uow.MemberRepository.GetMemberByIdAsync(currentUserId);
                if (currentUser?.Location == null) return BadRequest("Your location is not available to filter by distance.");
                
                var membersWithDistance = await memberService.GetMembersWithFiltersAsync(memberParams, currentUser.Location);
                return Ok(membersWithDistance);
            }
            
            memberParams.CurrentMemberId = User.GetMemberId();
            var members = await memberService.GetMembersWithFiltersAsync(memberParams, null);
            return Ok(members);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(MemberDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<MemberDto>> GetMember(string id)
        {
            var member = await uow.MemberRepository.GetMemberDtoByIdAsync(id);

            if (member == null) return NotFound();
            return Ok(member);

        }

        [HttpGet("{id}/photos")]
        public async Task<ActionResult<IReadOnlyList<PhotoDto>>> GetMemberPhotos(string id)
        {
            var isCurrentUser = User.GetMemberId() == id;
            return Ok(await uow.MemberRepository.GetPhotosForMemberAsync(id, isCurrentUser));
        }

        [HttpPut]
        public async Task<ActionResult> UpdateMember(MemberUpdateDto memberUpdateDto)
        {
            var result = await memberService.UpdateMemberAsync(User.GetMemberId(), memberUpdateDto);
            
            if (result) return NoContent();

            return BadRequest("Failed to update member");

        }

        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto( IFormFile file)
        {
            var member = await uow.MemberRepository.GetMemberForUpdate(User.GetMemberId());

            if (member == null) return BadRequest("Cannot update member");
            var result = await photoService.UploadPhotoAsync(file.OpenReadStream(), file.FileName);

            if (result == null) return BadRequest("Problem uploading photo");

            var photo = new Photo
            {
                Url = result.Url,
                PublicId = result.PublicId,
                MemberId = User.GetMemberId(),
                IsApproved = false

            };

            member.Photos.Add(photo);

            if (await uow.Complete())
            {
                // A PhotoDto-t adjuk vissza, nem az entitást
                var photoDto = new PhotoDto { Id = photo.Id, Url = photo.Url, IsApproved = photo.IsApproved, IsMain = false };
                // A CreatedAtRoute lenne a legtisztább, de ehhez kellene egy GetPhoto action
                return Ok(photoDto);
            }

            return BadRequest("Problem adding photo");
        }

        [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult> SetMainPhoto(int photoId)
        {
            var result = await memberService.SetMainPhotoAsync(User.GetMemberId(), photoId);
            
            if (result) return NoContent();

            return BadRequest("Problem setting main photo");
        }

        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            var result = await photoService.DeleteMemberPhotoAsync(User.GetMemberId(), photoId);
            
            if (result) return Ok();

            return BadRequest("Problem deleting the photo");
        }
    }
}
