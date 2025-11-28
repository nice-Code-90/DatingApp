using DatingApp.Application.DTOs;
using DatingApp.Application.Extensions;
using DatingApp.Application.Interfaces;
using DatingApp.Application.Helpers;
using DatingApp.Domain.Entities;

namespace DatingApp.Application.Services;

public class MessageService(IUnitOfWork uow, ICurrentUserService currentUserService) : IMessageService
{
    public async Task<MessageDto?> CreateMessageAsync(CreateMessageDto createMessageDto)
    {
        var senderId = currentUserService.MemberId;
        if (string.IsNullOrEmpty(senderId)) return null;

        var sender = await uow.MemberRepository.GetMemberByIdAsync(senderId);
        var recipient = await uow.MemberRepository.GetMemberByIdAsync(createMessageDto.RecipientId);

        if (recipient == null || sender == null || sender.Id == createMessageDto.RecipientId)
        {
            return null; 
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

    public async Task<bool> DeleteMessageAsync(string messageId)
    {
        var currentUserId = currentUserService.MemberId;
        if (string.IsNullOrEmpty(currentUserId)) return false;

        var message = await uow.MessageRepository.GetMessage(messageId);
        if (message == null) return false;

        if (message.SenderId != currentUserId && message.RecipientId != currentUserId)
            return false; 

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
        var memberId = currentUserService.MemberId;
        if (string.IsNullOrEmpty(memberId))
        {
            return PaginatedResult<MessageDto>.Empty(messageParams.PageNumber, messageParams.PageSize);
        }
        messageParams.MemberId = memberId;

        return await uow.MessageRepository.GetMessagesForMemberAsync(messageParams);
    }

    public async Task<IReadOnlyList<MessageDto>> GetMessageThread(string recipientId)
    {
        var currentMemberId = currentUserService.MemberId;
        if (string.IsNullOrEmpty(currentMemberId)) return new List<MessageDto>();

        return await uow.MessageRepository.GetMessageThread(currentMemberId, recipientId);
    }
}