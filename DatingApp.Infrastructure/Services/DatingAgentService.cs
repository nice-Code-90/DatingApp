using DatingApp.Application.DTOs;
using DatingApp.Application.Helpers;
using DatingApp.Application.Interfaces;
using DatingApp.Infrastructure.Extensions;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

public class DatingAgentService(
    IChatClient chatClient,
    IDatingAgentTools tools,
    ICurrentUserService currentUserService, // <-- Új injektálás
    ILogger<DatingAgentService> logger) : IDatingAgentService
{
    // A 'currentUserId' paramétert töröltük a szignatúrából
    public async Task<Result<AgentResponseDto>> ProcessAgentIntentAsync(string userPrompt)
    {
        // Opcionális: korai ellenőrzés, ha biztosra akarunk menni
        if (string.IsNullOrEmpty(currentUserService.MemberId))
            return Result<AgentResponseDto>.Failure("User identification failed.");

        var agentTools = new List<AITool>
        {
            AIFunctionFactory.Create(tools.SearchMatches),
            AIFunctionFactory.Create(tools.LikeMember)
        };

        var agent = chatClient.CreateCerebrasAgent(
               instructions: "You are a proactive dating wingman. Your goal is to help users find matches and take actions. " +
              "CRITICAL: When presenting matches to the user, NEVER display the raw GUID in the text. " +
              "However, for navigation, you MUST format member names as Markdown links: [Name](/members/ID). " +
              "Format matches as a clean, friendly list. " +
              "EFFICIENCY: Use SearchMatches once to find candidates. If you decide to like someone, call LikeMember and then IMMEDIATELY respond to the user. " +
              "Do NOT call SearchMatches again after a successful LikeMember call.",
              tools: agentTools
);

        try
        {
            var response = await agent.RunAsync(userPrompt);
            var cleanMessage = response.GetCleanContent();
            var resultDto = response.ToDto(cleanMessage);

            return Result<AgentResponseDto>.Success(resultDto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Dating Agent failed to process intent: {Prompt}", userPrompt);
            return Result<AgentResponseDto>.Failure("The agent encountered an error while processing your request.");
        }
    }
}