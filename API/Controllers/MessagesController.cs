using System;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace API.Controllers;

public class MessagesController(IMessageRepository messageRepository,
    IMemberRepository memberRepository) : BaseApiController
{
    [HttpPost]
    public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto)
    {
        var sender = await memberRepository.GetMemberByIdAsync(User.GetMemberId());
        var recipient = await memberRepository.GetMemberByIdAsync(createMessageDto.RecipientId);

        if (recipient == null || sender == null || sender.Id == createMessageDto.RecipientId)
        {
            return BadRequest("Cannot send this message");
        }
        var message = new Message
        {
            SenderId = sender.Id,
            RecipientId = recipient.Id,
            Content = createMessageDto.Content
        };

        messageRepository.AddMessage(message);
        if (await messageRepository.SaveAllAsync()) return message.ToDto();

        return BadRequest("Failed to send message");
    }

}
