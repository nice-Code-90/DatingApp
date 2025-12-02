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

        public AiHelperController(
            IAiHelperService aiHelperService,
            IAiMatchmakingService aiMatchmakingService)
        {
            _aiHelperService = aiHelperService;
            _aiMatchmakingService = aiMatchmakingService;
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

            var members = await _aiMatchmakingService.FindMatchingMembersAsync(query);

            if (!members.Any()) return NotFound("No matches found based on your description.");

            return Ok(members);
        }
    }
}