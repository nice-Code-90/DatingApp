using DatingApp.Application.DTOs;
using DatingApp.Domain.Entities;
using DatingApp.Application.Helpers;

namespace DatingApp.Application.Interfaces
{
    public interface IAiMatchmakingService
    {
        Task InitCollectionAsync();
        Task UpdateMemberProfileAsync(Member member);
        Task<Dictionary<string, float>> FindMatchesWithScoresAsync(AiSearchParams searchParams);
        Task<Result<IEnumerable<MemberDto>>> FindMatchingMembersAsync(AiSearchParams searchParams);

    }
}