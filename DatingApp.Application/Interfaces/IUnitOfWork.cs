using System;

namespace DatingApp.Application.Interfaces;

public interface IUnitOfWork
{
    IMemberRepository MemberRepository { get; }
    ILikesRepository LikesRepository { get; }
    IMessageRepository MessageRepository { get; }
    IPhotoRepository PhotoRepository { get; }
    Task<bool> Complete();
    bool HasChanges();
}
