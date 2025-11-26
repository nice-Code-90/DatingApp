using DatingApp.Application.DTOs;
using DatingApp.Application.Extensions;
using DatingApp.Application.Interfaces;
using DatingApp.Application.Helpers;
using DatingApp.Domain.Entities;
using DatingApp.Infrastructure.Data;

namespace DatingApp.Infrastructure.Services;

public class MessageService(IUnitOfWork uow) : IMessageService
{
    public async Task<MessageDto?> CreateMessageAsync(string senderId, CreateMessageDto createMessageDto)
    {
        var sender = await uow.MemberRepository.GetMemberByIdAsync(senderId);
        var recipient = await uow.MemberRepository.GetMemberByIdAsync(createMessageDto.RecipientId);

        if (recipient == null || sender == null || sender.Id == createMessageDto.RecipientId)
        {
            return null; // Controller will handle BadRequest
        }

        var message = new Message
        {
            SenderId = sender.Id,
            RecipientId = recipient.Id,
            Content = createMessageDto.Content
        };

        uow.MessageRepository.AddMessage(message);

        if (await uow.Complete()) return message.ToDto();

        return null;
    }

    public async Task<bool> DeleteMessageAsync(string currentUserId, string messageId)
    {
        var message = await uow.MessageRepository.GetMessage(messageId);
        if (message == null) return false;

        if (message.SenderId != currentUserId && message.RecipientId != currentUserId)
            return false; // User has no permission

        if (message.SenderId == currentUserId) message.SenderDeleted = true;
        if (message.RecipientId == currentUserId) message.RecipientDeleted = true;

        if (message is { SenderDeleted: true, RecipientDeleted: true })
        {
            uow.MessageRepository.DeleteMessage(message);
        }

        return await uow.Complete();
    }

    public async Task<PaginatedResult<MessageDto>> GetMessagesForMemberAsync(MessageParams messageParams)
    {
        var query = uow.MessageRepository.GetMessagesAsQueryable();

        query = messageParams.Container switch
        {
            "Outbox" => query.Where(x => x.SenderId == messageParams.MemberId
                && x.SenderDeleted == false),
            _ => query.Where(x => x.RecipientId == messageParams.MemberId 
                && x.RecipientDeleted == false)
        };

        var messageQuery = query.Select(MessageExtensions.ToDtoProjection());

        return await PaginationHelper.CreateAsync(messageQuery, messageParams.PageNumber, messageParams.PageSize);
    }
}