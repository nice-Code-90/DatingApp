using System.Security.Claims;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class MembersController(IMemberRepository memberRepository) : BaseApiController
    {
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<Member>>> GetMembers()
        {

            return Ok(await memberRepository.GetMembersAsync());
        }

        [HttpGet("{id}")]  // localhost:5001/api/members/bob-id
        public async Task<ActionResult<Member>> GetMember(string id)
        {
            var member = await memberRepository.GetMemberByIdAsync(id);

            if (member == null) return NotFound();
            return member;

        }

        [HttpGet("{id}/photos")]
        public async Task<ActionResult<IReadOnlyList<Photo>>> GetMemberPhotos(string id)
        {
            return Ok(await memberRepository.GetPhotosForMemberAsync(id));
        }

        [HttpPut]
        public async Task<ActionResult> UpdateMember(MemberUpdateDto memberUpdateDto)
        {
            var memberId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (memberId == null) return BadRequest("Oops - no id found in token");

            var member = await memberRepository.GetMemberByIdAsync(memberId);
            if (member == null) return BadRequest("Couldn't get member");

            member.DisplayName = memberUpdateDto.DisplayName ?? member.DisplayName;
            member.Description = memberUpdateDto.Description ?? member.Description;
            member.City = memberUpdateDto.City ?? member.City;
            member.Country = memberUpdateDto.Country ?? member.Country;

            //memberRepository.Update(member);
            if (await memberRepository.SaveAllAsync()) return NoContent();

            return BadRequest("Failed to update member");

        }
    }
}
