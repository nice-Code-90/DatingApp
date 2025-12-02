using DatingApp.Application.Interfaces;
using DatingApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using DatingApp.Domain.Entities;

namespace DatingApp.Infrastructure.Repository;

public class UnitOfWork(
    AppDbContext context,
    IMemberRepository memberRepository,
    IMessageRepository messageRepository,
    ILikesRepository likesRepository,
    IPhotoRepository photoRepository,
    IUserRepository userRepository) : IUnitOfWork
{
    public IMemberRepository MemberRepository { get; } = memberRepository;
    public IMessageRepository MessageRepository { get; } = messageRepository;
    public ILikesRepository LikesRepository { get; } = likesRepository;
    public IPhotoRepository PhotoRepository { get; } = photoRepository;
    public IUserRepository UserRepository { get; } = userRepository;

    public async Task<bool> Complete()
    {
        try
        {
            return await context.SaveChangesAsync() > 0;
        }
        catch (DbUpdateException ex)
        {
            throw new Exception("An error occured while saving changes", ex);
        }
    }

    public bool HasChanges()
    {
        return context.ChangeTracker.HasChanges();
    }
}
