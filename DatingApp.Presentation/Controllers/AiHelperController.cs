using System.Security.Claims;
using DatingApp.Application.DTOs;
using DatingApp.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using DatingApp.Application.Extensions;

namespace DatingApp.Presentation.Controllers
{
    [Route("api/[controller]")]
    public class AiHelperController : BaseApiController
    {
        private readonly IAiHelperService _aiHelperService;

        private readonly IAiMatchmakingService _aiMatchmakingService;
        private readonly IUnitOfWork _unitOfWork;

        public AiHelperController(
            IAiHelperService aiHelperService,
            IAiMatchmakingService aiMatchmakingService,
            IUnitOfWork unitOfWork)
        {
            _aiHelperService = aiHelperService;
            _aiMatchmakingService = aiMatchmakingService;
            _unitOfWork = unitOfWork;
        }

        [HttpGet("suggestion/{recipientId}")]
        public async Task<ActionResult<string>> GetSuggestion(string recipientId)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(currentUserId)) return Unauthorized();

            var suggestion = await _aiHelperService.GetChatSuggestion(currentUserId, recipientId);

            return Ok(new { suggestion = suggestion });
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<MemberDto>>> SmartSearch([FromQuery] string query)
        {
            if (string.IsNullOrEmpty(query)) return BadRequest("Search query cannot be empty");

            var matchIds = await _aiMatchmakingService.FindMatchesIdsAsync(query);

            if (!matchIds.Any()) return NotFound("No matches found based on your description.");

            var members = new List<MemberDto>();

            foreach (var id in matchIds)
            {
                var member = await _unitOfWork.UserRepository.GetMemberByIdAsync(id);

                if (member != null)
                {
                    members.Add(new MemberDto
                    {
                        Id = member.Id,
                        DisplayName = member.DisplayName,
                        ImageUrl = member.ImageUrl,
                        Age = member.DateOfBirth.CalculateAge(),
                        City = member.City,
                        Country = member.Country,
                        Gender = member.Gender,
                    });
                }
            }

            return Ok(members);
        }
    }
}