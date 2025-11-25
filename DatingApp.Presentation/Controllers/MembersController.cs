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
    IPhotoService photoService) : BaseApiController
    {
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<Member>>> GetMembers(
            [FromQuery] MemberParams memberParams)
        {

            memberParams.CurrentMemberId = User.GetMemberId();
 
            var currentUser = await uow.MemberRepository.GetMemberByIdAsync(memberParams.CurrentMemberId);
 
            if (currentUser == null) return Unauthorized("User profile not found.");
 
            var members = await uow.MemberRepository.GetMembersAsync(memberParams, currentUser.Location);
 
            return Ok(members);
            
        }

        [HttpGet("{id}")]  
        public async Task<ActionResult<Member>> GetMember(string id)
        {
            var member = await uow.MemberRepository.GetMemberByIdAsync(id);

            if (member == null) return NotFound();
            return member;

        }

        [HttpGet("{id}/photos")]
        public async Task<ActionResult<IReadOnlyList<Photo>>> GetMemberPhotos(string id)
        {
            var isCurrentUser = User.GetMemberId() == id;
            return Ok(await uow.MemberRepository.GetPhotosForMemberAsync(id, isCurrentUser));
        }

        [HttpPut]
        public async Task<ActionResult> UpdateMember(MemberUpdateDto memberUpdateDto)
        {
            var memberId = User.GetMemberId();

            var member = await uow.MemberRepository.GetMemberForUpdate(memberId);
            if (member == null) return BadRequest("Couldn't get member");

            member.DisplayName = memberUpdateDto.DisplayName ?? member.DisplayName;
            member.Description = memberUpdateDto.Description ?? member.Description;
            member.City = memberUpdateDto.City ?? member.City;
            member.Country = memberUpdateDto.Country ?? member.Country;

            member.User.DisplayName = memberUpdateDto.DisplayName ?? member.User.DisplayName;


            uow.MemberRepository.Update(member);
            if (await uow.Complete()) return NoContent();

            return BadRequest("Failed to update member");

        }

        [HttpPost("add-photo")]
        public async Task<ActionResult<Photo>> AddPhoto( IFormFile file)
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
                IsApproved = true

            };

            member.Photos.Add(photo);

            if (await uow.Complete()) return photo;
            return BadRequest("Problem adding photo");
        }

        [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult> SetMainPhoto(int photoId)
        {
            var member = await uow.MemberRepository.GetMemberForUpdate(User.GetMemberId());

            if (member == null) return BadRequest("Cannot get member from token");

            var photo = member.Photos.SingleOrDefault(x => x.Id == photoId);

            if (member.ImageUrl == photo?.Url || photo == null)
            {
                return BadRequest("Cannot set this as main image");
            }
            member.ImageUrl = photo.Url;
            member.User.ImageUrl = photo.Url;

            if (await uow.Complete()) return NoContent();

            return BadRequest("Problem setting main photo");
        }

        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            var member = await uow.MemberRepository.GetMemberForUpdate(User.GetMemberId());

            if (member == null) return BadRequest("Cannot get member from token");

            var photo = member.Photos.SingleOrDefault(x => x.Id == photoId);
            if (photo == null || photo.Url == member.ImageUrl)
            {
                return BadRequest("This photo cannot be deleted");
            }
            if (photo.PublicId != null)
            {
                var result = await photoService.DeletePhotoAsync(photo.PublicId);
                if (!result) return BadRequest("Problem deleting photo from cloud");
            }

            member.Photos.Remove(photo);

            if (await uow.Complete()) return Ok();

            return BadRequest("Problem deleting the photo");
        }
    }


}
