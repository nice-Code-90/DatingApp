using System.ComponentModel;
using DatingApp.Application.Interfaces;
using DatingApp.Application.Helpers;

namespace DatingApp.Application.Services;

public class DatingAgentTools : IDatingAgentTools
{
    private readonly IAiMatchmakingService _matchmakingService;
    private readonly ILikesService _likesService;
    private readonly ICurrentUserService _currentUserService;

    public DatingAgentTools(
        IAiMatchmakingService matchmakingService,
        ILikesService likesService,
        ICurrentUserService currentUserService)
    {
        _matchmakingService = matchmakingService;
        _likesService = likesService;
        _currentUserService = currentUserService;
    }

    private string CurrentUserId => _currentUserService.MemberId
        ?? throw new UnauthorizedAccessException("User context is missing. Please log in.");

    [Description("Searches for potential dating matches based on a natural language query. Can filter by gender.")]
    public async Task<string> SearchMatches(string query, string? gender = null)
    {
        var result = await _matchmakingService.FindMatchingMembersAsync(new AiSearchParams
        {
            Query = query,
            Gender = gender
        });

        if (!result.IsSuccess || !result.Value.Any())
            return "No matches found.";
        return string.Join(", ", result.Value.Take(3).Select(m => $"{m.DisplayName} (ID: {m.Id})"));
    }


    [Description("Likes a specific member by their ID. Only use this if the user hasn't liked them yet.")]
    public async Task<string> LikeMember(string targetMemberId)
    {
        
        var likedIds = await _likesService.GetCurrentMemberLikeIds(); 

        if (likedIds.Contains(targetMemberId)) 
        {
            return "Info: You have already liked this member.";
        }

        var success = await _likesService.ToggleLikeAsync(targetMemberId); 

        return success
            ? "Success: Member has been liked."
            : "Error: Could not perform like action.";
    }
}