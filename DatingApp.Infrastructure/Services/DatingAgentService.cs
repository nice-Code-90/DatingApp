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
          instructions: "You are a proactive dating wingman. Follow these rules strictly:\n" +
                        "1. LINKING: Every time you mention a member's name, you MUST use this format: [DisplayName](/members/Id). Use the ID returned by the search tool.\n" +
                        "2. NO RAW IDS: Never display a GUID/ID as plain text. Only use it inside the Markdown link.\n" +
                        "3. SEARCH LIMIT: Call 'SearchMatches' exactly ONCE. Use the results from that single call to help the user.\n" +
                        "4. STOPPING: If you call 'LikeMember', it must be your LAST tool call. Do not search again after liking someone.\n" +
                        "5. STYLE: Be friendly and encouraging, but prioritize the correct link formatting.",
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