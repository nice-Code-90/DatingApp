using DatingApp.Application.DTOs;
using DatingApp.Application.Extensions;
using DatingApp.Application.Interfaces;
using DatingApp.Application.Helpers;
using DatingApp.Domain.Entities;

namespace DatingApp.Application.Services;

public class MessageService(IUnitOfWork uow, ICurrentUserService currentUserService) : IMessageService
{
    public async Task<Result<MessageDto>> CreateMessageAsync(CreateMessageDto createMessageDto)
    {
        var senderId = currentUserService.MemberId;
        if (string.IsNullOrEmpty(senderId)) return Result<MessageDto>.Failure("Sender not found");

        var sender = await uow.MemberRepository.GetMemberByIdAsync(senderId);
        var recipient = await uow.MemberRepository.GetMemberByIdAsync(createMessageDto.RecipientId);

        if (recipient == null || sender == null || sender.Id == createMessageDto.RecipientId)
        {
            return Result<MessageDto>.Failure("Cannot send message to this user");
        }

        var message = new Message
        {
            SenderId = sender.Id,
            RecipientId = recipient.Id,
            Content = createMessageDto.Content
        };

        uow.MessageRepository.AddMessage(message);

        if (await uow.Complete()) return Result<MessageDto>.Success(message.ToDto());

        return Result<MessageDto>.Failure("Failed to send message");
    }

    public async Task<Result<object>> DeleteMessageAsync(string messageId)
    {
        var currentUserId = currentUserService.MemberId;
        if (string.IsNullOrEmpty(currentUserId)) return Result.Failure("User not found");

        var message = await uow.MessageRepository.GetMessage(messageId);
        if (message == null) return Result.Failure("Message not found");

        if (message.SenderId != currentUserId && message.RecipientId != currentUserId)
            return Result.Failure("Unauthorized to delete this message");

        if (message.SenderId == currentUserId) message.SenderDeleted = true;
        if (message.RecipientId == currentUserId) message.RecipientDeleted = true;

        if (message is { SenderDeleted: true, RecipientDeleted: true })
        {
            uow.MessageRepository.DeleteMessage(message);
        }

        return await uow.Complete() ? Result<object>.Success(new { }) : Result.Failure("Failed to delete message");
    }

    public async Task<Result<PaginatedResult<MessageDto>>> GetMessagesForMemberAsync(MessageParams messageParams)
    {
        var memberId = currentUserService.MemberId;
        if (string.IsNullOrEmpty(memberId))
        {
            return Result<PaginatedResult<MessageDto>>.Failure("User not found");
        }
        messageParams.MemberId = memberId;

        var messages = await uow.MessageRepository.GetMessagesForMemberAsync(messageParams);
        return Result<PaginatedResult<MessageDto>>.Success(messages);
    }

    public async Task<Result<IReadOnlyList<MessageDto>>> GetMessageThread(string recipientId)
    {
        var currentMemberId = currentUserService.MemberId;
        if (string.IsNullOrEmpty(currentMemberId)) return Result<IReadOnlyList<MessageDto>>.Failure("User not found");

        var messages = await uow.MessageRepository.GetMessageThread(currentMemberId, recipientId);
        return Result<IReadOnlyList<MessageDto>>.Success(messages);
    }
}