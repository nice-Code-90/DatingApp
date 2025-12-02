using DatingApp.Application.DTOs;
using DatingApp.Domain.Entities;
using DatingApp.Application.Helpers;

namespace DatingApp.Application.Interfaces
{
    public interface IAiMatchmakingService
    {
        Task InitCollectionAsync();
        Task UpdateMemberProfileAsync(Member member);
        Task<IEnumerable<string>> FindMatchesIdsAsync(string searchQuery);
        Task<Result<IEnumerable<MemberDto>>> FindMatchingMembersAsync(string searchQuery);

    }
}