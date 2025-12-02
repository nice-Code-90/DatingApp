using DatingApp.Application.Interfaces;
using DatingApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using DatingApp.Domain.Entities; 

namespace DatingApp.Infrastructure.Repository;

public class UnitOfWork(AppDbContext context, UserManager<AppUser> userManager) : IUnitOfWork{
    private IMemberRepository? _memberRepository;
    private IMessageRepository? _messageRepository;
    private ILikesRepository? _likesRepository;
    private IPhotoRepository? _photoRepository;
    private IUserRepository? _userRepository;

    public IMemberRepository MemberRepository => _memberRepository
        ??= new MemberRepository(context);
    public IMessageRepository MessageRepository => _messageRepository
        ??= new MessageRepository(context);
    public ILikesRepository LikesRepository => _likesRepository
        ??= new LikesRepository(context);
    public IPhotoRepository PhotoRepository => _photoRepository
   ??= new PhotoRepository(context);

   public IUserRepository UserRepository => _userRepository
        ??= new UserRepository(userManager, context);

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
