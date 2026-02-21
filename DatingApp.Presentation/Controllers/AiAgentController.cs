using DatingApp.Application.DTOs;
using DatingApp.Application.Extensions; 
using DatingApp.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.Presentation.Controllers;

[Authorize]
public class AiAgentController(IDatingAgentService agentService) : BaseApiController
{
    [HttpPost("process")]
    public async Task<ActionResult<AgentResponseDto>> ProcessCommand([FromBody] ChatPromptDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Prompt)) return BadRequest("Prompt cannot be empty");
        return HandleResult(await agentService.ProcessAgentIntentAsync(dto.Prompt));
    }
}