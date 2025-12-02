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
        return HandleResult(await messageService.CreateMessageAsync(createMessageDto));
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedResult<MessageDto>>> GetMessagesForUser(
        [FromQuery] MessageParams messageParams)
    {
        return HandleResult(await messageService.GetMessagesForMemberAsync(messageParams));
    }

    [HttpGet("thread/{recipientId}")]
    public async Task<ActionResult<IReadOnlyList<MessageDto>>> GetMessageThread(string recipientId)
    {
        return HandleResult(await messageService.GetMessageThread(recipientId));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteMessage(string id)
    {
        return HandleResult(await messageService.DeleteMessageAsync(id));
    }
}
