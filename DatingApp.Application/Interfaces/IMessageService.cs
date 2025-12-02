using DatingApp.Application.DTOs;
using DatingApp.Application.Helpers;

namespace DatingApp.Application.Interfaces;

public interface IMessageService
{
    Task<Result<MessageDto>> CreateMessageAsync(CreateMessageDto createMessageDto);
    Task<Result<PaginatedResult<MessageDto>>> GetMessagesForMemberAsync(MessageParams messageParams);
    Task<Result<IReadOnlyList<MessageDto>>> GetMessageThread(string recipientId);
    Task<Result<object>> DeleteMessageAsync(string id);
}