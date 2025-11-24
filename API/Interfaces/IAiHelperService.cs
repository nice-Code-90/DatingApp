using System.Threading.Tasks;

namespace API.Interfaces
{
    public interface IAiHelperService
    {
        Task<string> GetChatSuggestion(string currentUserId, string recipientId);
    }
}