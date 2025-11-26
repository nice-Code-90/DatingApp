using System.Text;
using DatingApp.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Google.GenAI;
using Microsoft.Extensions.Options;

namespace DatingApp.Infrastructure.Services
{
    public class AiHelperService : IAiHelperService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AiHelperService> _logger;
        private readonly GeminiSettings _geminiSettings;

        public AiHelperService(IUnitOfWork unitOfWork, IOptions<GeminiSettings> config, ILogger<AiHelperService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _geminiSettings = config.Value;
        }

        public async Task<string> GetChatSuggestion(string currentUserId, string recipientId)
        {

            var currentUser = await _unitOfWork.MemberRepository.GetMemberByIdAsync(currentUserId);
            var recipient = await _unitOfWork.MemberRepository.GetMemberByIdAsync(recipientId);
            var messageThread = await _unitOfWork.MessageRepository.GetMessageThread(currentUserId, recipientId);

            if (recipient == null || currentUser == null)
            {   _logger.LogWarning("AI chat suggestion requested for non-existent user(s). CurrentUser: {CurrentUser}, Recipient: {Recipient}", currentUserId, recipientId);
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


            if (string.IsNullOrEmpty(_geminiSettings.ApiKey))
            {
                _logger.LogError("Gemini API key not found in configuration.");
                return "Sorry, AI suggestion is currently unavailable due to missing API key.";
            }
            
            try {
                var client = new Client(apiKey: _geminiSettings.ApiKey);

                var response = await client.Models.GenerateContentAsync("gemini-2.5-flash", promptBuilder.ToString());
                var suggestion = response.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;
                return suggestion ?? "Sorry, I couldn't come up with a suggestion right now.";
            } catch (Exception ex) {
                _logger.LogError(ex, "Error calling Gemini API for chat suggestion.");
                return "Sorry, I couldn't come up with a suggestion right now due to an internal error.";
            }
        }
    }
}