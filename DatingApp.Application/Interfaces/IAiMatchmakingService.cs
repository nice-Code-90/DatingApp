using DatingApp.Application.DTOs;
using DatingApp.Application.Helpers;
using DatingApp.Domain.Entities;


namespace DatingApp.Application.Interfaces
{
    public interface IAiMatchmakingService
    {
        Task InitCollectionAsync();
        Task UpdateMemberProfileAsync(Member member);
        Task<Dictionary<string, float>> FindMatchesWithScoresAsync(AiSearchParamsDto searchParams);
        Task<Result<IEnumerable<MemberDto>>> FindMatchingMembersAsync(AiSearchParamsDto searchParams);

    }
}