using DatingApp.Application.DTOs;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using System.Linq;

namespace DatingApp.Infrastructure.Extensions;

public static class AgentMappingExtensions
{
    public static AgentResponseDto ToDto(this AgentResponse response, string cleanedContent)
    {
        var dto = new AgentResponseDto
        {
            Message = cleanedContent,
            ActionsPerformed = new List<string>(),
            AffectedTargetIds = new List<string>()
        };

        
        var functionCalls = response.Messages
            .Where(m => m.Role == ChatRole.Assistant)
            .SelectMany(m => m.Contents)
            .OfType<FunctionCallContent>()
            .Where(call => !string.IsNullOrWhiteSpace(call.Name));

        foreach (var call in functionCalls)
        {
            dto.ActionsPerformed.Add(call.Name);

            
            if (call is { Name: "LikeMember", Arguments: not null } &&
                call.Arguments.TryGetValue("targetMemberId", out var idObj) &&
                idObj?.ToString() is { Length: > 0 } targetId)
            {
                dto.AffectedTargetIds.Add(targetId);
            }
        }

        
        if (dto.ActionsPerformed.Count == 0)
        {
            dto.ActionsPerformed.Add("Conversation");
        }

        return dto;
    }
}