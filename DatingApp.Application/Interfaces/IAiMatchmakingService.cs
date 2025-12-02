using DatingApp.Application.DTOs;
using DatingApp.Domain.Entities;

namespace DatingApp.Application.Interfaces
{
    public interface IAiMatchmakingService
    {
        Task InitCollectionAsync();
        Task UpdateMemberProfileAsync(Member member);
        Task<IEnumerable<string>> FindMatchesIdsAsync(string searchQuery);
        Task<IEnumerable<MemberDto>> FindMatchingMembersAsync(string searchQuery);

    }
}