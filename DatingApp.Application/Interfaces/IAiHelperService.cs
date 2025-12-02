using DatingApp.Application.DTOs;
using DatingApp.Application.Helpers;

namespace DatingApp.Application.Interfaces;

public interface IAiHelperService
{
    Task<Result<SuggestionDto>> GetChatSuggestion(string currentUserId, string recipientId);
}