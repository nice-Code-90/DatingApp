using System.ComponentModel;

namespace DatingApp.Application.Interfaces;

public interface IDatingAgentTools
{
    [Description("Searches for matches based on user interests, personality, or physical traits.")]
    Task<string> SearchMatches(string query, string? gender = null);

    [Description("Likes a specific member by their ID. This is used to express interest.")]
    Task<string> LikeMember(string targetMemberId);
}