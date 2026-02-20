namespace DatingApp.Application.DTOs;

public class AgentResponseDto
{
    
    public string Message { get; set; } = string.Empty;

    
    public List<string> ActionsPerformed { get; set; } = new();

    
    public List<string> AffectedTargetIds { get; set; } = new();
}