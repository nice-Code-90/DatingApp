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
                  "CRITICAL: When presenting matches to the user, NEVER display the Member ID (GUID). " +
                  "Keep the IDs for your internal tool usage only. Refer to members only by their Display Name. " +
                  "Format the matches as a clean, friendly list without technical jargon." +
                  "Try to find matches with a single search. Do not call SearchMatches repeatedly if you already have suitable candidates.",
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