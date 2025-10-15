using Microsoft.EntityFrameworkCore;
using Snapdi.Repositories.Interfaces;
using Snapdi.Services.DTOs;
using Snapdi.Services.Interfaces;
using Snapdi.Services.Interfaces.Snapdi.Services.Interfaces;
using Snapdi.Services.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snapdi.Services.Services
{
    public class MessageService : IMessageService
    {
        private readonly IMessageRepository _messageRepository;
        private readonly IConversationRepository _conversationRepository;
        private readonly IUserRepository _userRepository;

        public MessageService(
            IMessageRepository messageRepository,
            IConversationRepository conversationRepository,
            IUserRepository userRepository)
        {
            _messageRepository = messageRepository;
            _conversationRepository = conversationRepository;
            _userRepository = userRepository;
        }

        public async Task<IEnumerable<ConversationDto>> GetUserConversationsAsync(int userId)
        {
            var conversations = await _conversationRepository.GetUserConversationsAsync(userId);
            if (conversations is IEnumerable<ConversationDto> dtos)
                return dtos;

            // Example mapping if needed (replace with your actual mapping logic)
            return conversations.Select(c => new ConversationDto
            {
                // Map properties from Conversation to ConversationDto
                // ConversationId = c.ConversationId,
                // Name = c.Name,
                // etc.
            });
        }

        public async Task<IEnumerable<MessageDto>> GetConversationMessagesAsync(
    int conversationId, int userId, int? beforeMessageId = null, int take = 50)
        {
            var isParticipant = await _conversationRepository.IsUserParticipantAsync(conversationId, userId);
            if (!isParticipant)
                throw new UnauthorizedAccessException("User is not a participant in this conversation");

            var messages = await _messageRepository.GetConversationMessagesAsync(conversationId, beforeMessageId, Math.Clamp(take, 1, 200));
            return messages.Select(message => new MessageDto
            {
                MessageId = message.MessageId,
                ConversationId = message.ConversationId ?? 0,
                SenderId = message.SenderId ?? 0,
                Content = message.Content,
                SendAt = message.SendAt
            });
        }

        public async Task<MessageDto> SendMessageAsync(int conversationId, int senderId, string content)
        {
            if (string.IsNullOrWhiteSpace(content) || content.Length > 2000)
                throw new ArgumentException("Invalid content");

            var isParticipant = await _conversationRepository.IsUserParticipantAsync(conversationId, senderId);
            if (!isParticipant)
                throw new UnauthorizedAccessException("User is not a participant in this conversation");

            var message = await _messageRepository.CreateMessageAsync(conversationId, senderId, content.Trim());
            return new MessageDto
            {
                MessageId = message.MessageId,
                ConversationId = message.ConversationId ?? 0,
                SenderId = message.SenderId ?? 0,
                Content = message.Content,
                SendAt = message.SendAt
            };
        }

        public async Task<int> GetOrCreateAdminConversationAsync(int userId)
        {
            var adminUser = await _userRepository.GetAdminUserAsync();
            if (adminUser == null)
                throw new InvalidOperationException("No admin found");

            var existingConversationId = await _conversationRepository.GetSupportConversationAsync(userId, adminUser.UserId);
            if (existingConversationId.HasValue)
                return existingConversationId.Value;

            return await _conversationRepository.CreateSupportConversationAsync(userId, adminUser.UserId);
        }

        public async Task<bool> IsUserParticipantAsync(int conversationId, int userId)
        {
            return await _conversationRepository.IsUserParticipantAsync(conversationId, userId);
        }

        public async Task MarkMessageAsReadAsync(int conversationId, int userId, int messageId)
        {
            await _conversationRepository.UpdateLastReadMessageAsync(conversationId, userId, messageId);
        }
    }
}
