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
            ActionsPerformed = new List<string>()
        };

        foreach (var message in response.Messages)
        {
            if (message.Role == ChatRole.Assistant)
            {
                var functionCalls = message.Contents.OfType<FunctionCallContent>();

                foreach (var call in functionCalls)
                {
                    if (!string.IsNullOrEmpty(call.Name))
                    {
                        dto.ActionsPerformed.Add(call.Name);
                    }
                }
            }
        }

        if (!dto.ActionsPerformed.Any())
        {
            dto.ActionsPerformed.Add("Conversation");
        }

        return dto;
    }
}