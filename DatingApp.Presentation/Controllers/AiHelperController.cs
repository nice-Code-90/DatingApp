using System.Security.Claims;
using DatingApp.Application.DTOs;
using DatingApp.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DatingApp.Application.Extensions;

namespace DatingApp.Presentation.Controllers
{
    [Authorize]
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
        public async Task<ActionResult<SuggestionDto>> GetSuggestion(string recipientId)
        {
            var currentUserId = User.GetMemberId();

            return HandleResult(await _aiHelperService.GetChatSuggestion(currentUserId, recipientId));
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<MemberDto>>> SearchMembers([FromQuery] string query)
        {

            var result = await _aiMatchmakingService.FindMatchingMembersAsync(query);

            return HandleResult(result);
        }
    }
}