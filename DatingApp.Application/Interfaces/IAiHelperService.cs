using System.Threading.Tasks;

namespace DatingApp.Application.Interfaces;

    public interface IAiHelperService
    {
        Task<string> GetChatSuggestion(string currentUserId, string recipientId);
    }
