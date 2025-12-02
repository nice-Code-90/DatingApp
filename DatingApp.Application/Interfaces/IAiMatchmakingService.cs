using DatingApp.Domain.Entities;

namespace DatingApp.Application.Interfaces
{
    public interface IAiMatchmakingService
    {
        Task InitCollectionAsync();
        Task UpdateMemberProfileAsync(Member member);
        Task<IEnumerable<string>> FindMatchesIdsAsync(string searchQuery);
    }
}