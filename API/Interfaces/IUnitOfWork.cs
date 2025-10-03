using System;

namespace API.Interfaces;

public interface IUnitOfWork
{
    IMemberRepository MemberRepository { get; }
    ILikesRepository LikesRepository { get; }
    IMessageRepository MessageRepository { get; }
    Task<bool> Complete();
    bool HasChanges();
}
