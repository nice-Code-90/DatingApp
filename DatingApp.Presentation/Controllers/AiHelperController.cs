using System.Security.Claims;
using DatingApp.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.Presentation.Controllers
{
    [Route("api/[controller]")]
    public class AiHelperController : BaseApiController
    {
        private readonly IAiHelperService _aiHelperService;

        public AiHelperController(IAiHelperService aiHelperService)
        {
            _aiHelperService = aiHelperService;
        }

        [HttpGet("suggestion/{recipientId}")]
        public async Task<ActionResult<string>> GetSuggestion(string recipientId)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(currentUserId)) return Unauthorized();

            var suggestion = await _aiHelperService.GetChatSuggestion(currentUserId, recipientId);

            return Ok(new { suggestion = suggestion });
        }
    }
}
