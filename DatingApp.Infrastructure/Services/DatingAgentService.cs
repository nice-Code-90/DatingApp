using DatingApp.Application.DTOs;
using DatingApp.Application.Helpers;
using DatingApp.Application.Interfaces;
using DatingApp.Infrastructure.Extensions;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace DatingApp.Infrastructure.Services;

public class DatingAgentService(
    IChatClient chatClient,
    IDatingAgentTools tools,
    ILogger<DatingAgentService> logger) : IDatingAgentService
{
    public async Task<Result<AgentResponseDto>> ProcessAgentIntentAsync(string currentUserId, string userPrompt)
    {
        
        var agentTools = new List<AITool>
        {
            AIFunctionFactory.Create(tools.SearchMatches),
            AIFunctionFactory.Create(tools.LikeMember)
        };

        
        var agent = chatClient.CreateCerebrasAgent(
            instructions: "You are a proactive dating wingman. Your goal is to help users find matches and take actions. " +
                          "If a user asks to find someone, use SearchMatches. If they like someone, use LikeMember.",
            tools: agentTools
        );

        try
        {
            
            var response = await agent.RunAsync(userPrompt);
            var cleanMessage = response.GetCleanContent();

            
            var resultDto = new AgentResponseDto
            {
                Message = cleanMessage,
                
                ActionsPerformed = new List<string> { "Agentic Workflow Executed" }
            };

            return Result<AgentResponseDto>.Success(resultDto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Dating Agent failed to process intent: {Prompt}", userPrompt);
            return Result<AgentResponseDto>.Failure("The agent encountered an error while processing your request.");
        }
    }
}