using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DatingApp.Application.Interfaces;
using Google.GenAI;
using Microsoft.Extensions.Configuration;

namespace DatingApp.Infrastructure.Services
{
    public class AiHelperService : IAiHelperService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _config;

        public AiHelperService(IUnitOfWork unitOfWork, IConfiguration config)
        {
            _unitOfWork = unitOfWork;
            _config = config;
        }

        public async Task<string> GetChatSuggestion(string currentUserId, string recipientId)
        {

            var currentUser = await _unitOfWork.MemberRepository.GetMemberByIdAsync(currentUserId);
            var recipient = await _unitOfWork.MemberRepository.GetMemberByIdAsync(recipientId);
            var messageThread = await _unitOfWork.MessageRepository.GetMessageThread(currentUserId, recipientId);

            if (recipient == null || currentUser == null)
            {
                return "Error: User not found.";
            }


            var promptBuilder = new StringBuilder();
            promptBuilder.AppendLine("You are a helpful dating assistant. Your task is to help a user continue a conversation.");
            promptBuilder.AppendLine($"The user you are helping is '{currentUser.DisplayName}'.");
            promptBuilder.AppendLine($"They are talking to '{recipient.DisplayName}'. Here is some information about '{recipient.DisplayName}':");
            promptBuilder.AppendLine($"- Description: {recipient.Description}");
            promptBuilder.AppendLine("\nHere is the recent conversation history:");

            foreach (var message in messageThread.OrderBy(m => m.MessageSent).TakeLast(10))
            {
                promptBuilder.AppendLine($"- {message.SenderDisplayName}: {message.Content}");
            }

            promptBuilder.AppendLine($"\nBased on the information above, suggest a short, engaging, 2-3 sentence message for '{currentUser.DisplayName}' to send to continue the conversation. Be friendly and creative. Do not include a greeting like 'Hi' or 'Hello'.");


            var apiKey = _config["GeminiApiKey"] ?? throw new Exception("Gemini API key not found");
            var client = new Client(apiKey: apiKey);

            var response = await client.Models.GenerateContentAsync("gemini-2.5-flash", promptBuilder.ToString());
            var suggestion = response.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;

            return suggestion ?? "Sorry, I couldn't come up with a suggestion right now.";
        }
    }
}