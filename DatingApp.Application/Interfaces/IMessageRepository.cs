using DatingApp.Application.DTOs;
using DatingApp.Domain.Entities;
using DatingApp.Application.Helpers;

namespace DatingApp.Application.Interfaces;

public interface IMessageRepository
{
    void AddMessage(Message message);

    void DeleteMessage(Message message);

    Task<Message?> GetMessage(string messageId);

    Task<PaginatedResult<MessageDto>> GetMessagesForMemberAsync(MessageParams messageParams);
    Task<IReadOnlyList<MessageDto>> GetMessageThread(string currentMemberId, string recipientId);

    void AddGroup(Group group);

    Task RemoveConnection(string connectionId);

    Task<Connection?> GetConnection(string connectionId);

    Task<Group?> GetMessageGroup(string groupName);

    Task<Group?> GetGoupForConnection(string connectionId);
}