using DatingApp.Application.DTOs;

namespace DatingApp.Application.Interfaces;

public interface IMessageService
{
    Task<MessageDto?> CreateMessageAsync(string senderId, CreateMessageDto createMessageDto);
    Task<bool> DeleteMessageAsync(string currentUserId, string messageId);
}