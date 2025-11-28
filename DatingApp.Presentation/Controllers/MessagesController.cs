using DatingApp.Application.DTOs;
using DatingApp.Application.Extensions;
using DatingApp.Application.Helpers;
using DatingApp.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.Presentation.Controllers;

[Authorize]
public class MessagesController(IMessageService messageService) : BaseApiController
{
    [HttpPost]
    public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto)
    {
        var messageDto = await messageService.CreateMessageAsync(createMessageDto);

        if (messageDto != null) return Ok(messageDto);

        return BadRequest("Failed to send message");
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedResult<MessageDto>>> GetMessagesForUser(
        [FromQuery] MessageParams messageParams)
    {
        return Ok(await messageService.GetMessagesForMemberAsync(messageParams));
    }

    [HttpGet("thread/{recipientId}")]
    public async Task<ActionResult<IReadOnlyList<MessageDto>>> GetMessageThread(string recipientId)
    {
        return Ok(await messageService.GetMessageThread(recipientId));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteMessage(string id)
    {
        var result = await messageService.DeleteMessageAsync(id);

        if (result) return Ok();

        return BadRequest("Problem deleting the message");

    }
}
