using System.ComponentModel;
using DatingApp.Application.Interfaces;
using DatingApp.Application.Helpers; 

namespace DatingApp.Application.Services;

public class DatingAgentTools : IDatingAgentTools
{
    private readonly IAiMatchmakingService _matchmakingService;
    private readonly ILikesService _likesService;
    private readonly string _currentUserId; // from agent

    public DatingAgentTools(IAiMatchmakingService matchmakingService, ILikesService likesService, string currentUserId)
    {
        _matchmakingService = matchmakingService;
        _likesService = likesService;
        _currentUserId = currentUserId;
    }

    public async Task<string> SearchMatches(string query, string? gender = null)
    {
        var result = await _matchmakingService.FindMatchingMembersAsync(new AiSearchParams
        {
            Query = query,
            Gender = gender
        });

        if (!result.IsSuccess || !result.Value.Any()) return "No matches found.";

        return string.Join(", ", result.Value.Take(3).Select(m => $"{m.DisplayName} (ID: {m.Id})"));
    }

    [Description("Likes a specific member by their ID. Use this to express interest after finding a match.")]
    public async Task<string> LikeMember(string targetMemberId)
    {
        // LikesService handles the current user context internally
        var success = await _likesService.ToggleLikeAsync(targetMemberId);
        return success ? "Success: Member has been liked." : "Error: Could not perform like action.";
    }
}