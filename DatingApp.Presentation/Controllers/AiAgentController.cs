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
    [ProducesResponseType(typeof(AgentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AgentResponseDto>> ProcessCommand([FromBody] string prompt)
    {
        
        return HandleResult(await agentService.ProcessAgentIntentAsync(User.GetMemberId(), prompt));
    }
}