using System.Text;
using DatingApp.Application.DTOs;
using DatingApp.Application.Interfaces;
using DatingApp.Application.Helpers;
using Microsoft.Extensions.Logging;
using DatingApp.Infrastructure.Extensions;
using Microsoft.Extensions.AI;

namespace DatingApp.Infrastructure.Services
{
    public class AiHelperService : IAiHelperService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IChatClient _chatClient;
        private readonly ILogger<AiHelperService> _logger;

        public AiHelperService(IUnitOfWork unitOfWork, IChatClient chatClient, ILogger<AiHelperService> logger)
        {
            _unitOfWork = unitOfWork;
            _chatClient = chatClient;
            _logger = logger;
        }

        public async Task<Result<SuggestionDto>> GetChatSuggestion(string currentUserId, string recipientId)
        {
            var currentUser = await _unitOfWork.MemberRepository.GetMemberByIdAsync(currentUserId);
            var recipient = await _unitOfWork.MemberRepository.GetMemberByIdAsync(recipientId);
            var messageThread = await _unitOfWork.MessageRepository.GetMessageThread(currentUserId, recipientId);

            if (recipient == null || currentUser == null)
            {
                _logger.LogWarning("AI chat suggestion requested for non-existent user(s). CurrentUser: {CurrentUser}, Recipient: {Recipient}", currentUserId, recipientId);
                return Result<SuggestionDto>.Failure("Error: User not found.");
            }

            var conversationHistory = new StringBuilder();
            foreach (var message in messageThread.OrderBy(m => m.MessageSent).TakeLast(10))
            {
                conversationHistory.AppendLine($"- {message.SenderDisplayName}: {message.Content}");
            }

            var prompt = $"""
                You are a helpful dating assistant. Your task is to help a user continue a conversation.
                The user you are helping is '{currentUser.DisplayName}'.
                They are talking to '{recipient.DisplayName}'. Here is some information about '{recipient.DisplayName}':
                - Description: {recipient.Description ?? "No description provided."}

                Here is the recent conversation history:
                {conversationHistory}

                Based on the information above, suggest a short, engaging, 2-3 sentence message for '{currentUser.DisplayName}' to send to continue the conversation. Be friendly and creative. Do not include a greeting like 'Hi' or 'Hello'.
            """;

            try
            {
                var agent = _chatClient.CreateCerebrasAgent();
                var response = await agent.RunAsync(prompt);
                var suggestion = response.GetCleanContent();
                
                return Result<SuggestionDto>.Success(new SuggestionDto { Suggestion = suggestion });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Cerebras API for chat suggestion.");
                return Result<SuggestionDto>.Failure("Sorry, I couldn't come up with a suggestion right now due to an internal error.");
            }
        }
    }
}