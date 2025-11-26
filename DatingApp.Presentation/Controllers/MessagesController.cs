using DatingApp.Application.DTOs;
using DatingApp.Domain.Entities;
using DatingApp.Application.Extensions;
using DatingApp.Application.Helpers;
using DatingApp.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.Presentation.Controllers;

public class MessagesController(IUnitOfWork uow, IMessageService messageService) : BaseApiController
{
    [HttpPost]
    public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto)
    {
        var messageDto = await messageService.CreateMessageAsync(User.GetMemberId(), createMessageDto);

        if (messageDto != null) return Ok(messageDto);

        return BadRequest("Failed to send message");
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedResult<MessageDto>>> GetMessagesByContainer(
        [FromQuery] MessageParams messageParams)
    {
        messageParams.MemberId = User.GetMemberId();

        return Ok(await messageService.GetMessagesForMemberAsync(messageParams));
    }

    [HttpGet("thread/{recipientId}")]
    public async Task<ActionResult<IReadOnlyList<MessageDto>>> GetMessageThread(string recipientId)
    {
        return Ok(await uow.MessageRepository.GetMessageThread(User.GetMemberId(), recipientId));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteMessage(string id)
    {
        var result = await messageService.DeleteMessageAsync(User.GetMemberId(), id);

        if (result) return Ok();

        return BadRequest("Problem deleting the message");

    }
}
