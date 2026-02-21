using DatingApp.Application.DTOs;
using DatingApp.Application.Helpers;

namespace DatingApp.Application.Interfaces;

public interface IDatingAgentService
{
    /// <summary>
    /// Processes the user's free text command, 
    /// makes a decision, and executes the necessary tools.
    /// </summary>
    /// <param name="currentUserId">The ID of the logged-in user.</param>
    /// <param name="userPrompt">What the user requested (e.g., "Find sailors and like them").</param>
    /// <returns>The AI's text response about the actions performed.</returns>
    Task<Result<AgentResponseDto>> ProcessAgentIntentAsync(string userPrompt);
}