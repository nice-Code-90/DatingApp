using DatingApp.Application.DTOs;
using DatingApp.Application.Helpers;

namespace DatingApp.Application.Interfaces;

public interface IMessageService
{
    Task<PaginatedResult<MessageDto>> GetMessagesForMemberAsync(MessageParams messageParams);
    Task<MessageDto?> CreateMessageAsync(CreateMessageDto createMessageDto);
    Task<bool> DeleteMessageAsync(string messageId);
    Task<IReadOnlyList<MessageDto>> GetMessageThread(string recipientId);
}