using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Snapdi.Services.DTOs;
using Snapdi.Services.Models;

namespace Snapdi.Services.Interfaces
{
    namespace Snapdi.Services.Interfaces
    {
        public interface IMessageService
        {
            Task<IEnumerable<ConversationDto>> GetUserConversationsAsync(int userId);
            Task<IEnumerable<MessageDto>> GetConversationMessagesAsync(int conversationId, int userId, int? beforeMessageId = null, int take = 50);
            Task<MessageDto> SendMessageAsync(int conversationId, int senderId, string content);
            Task<int> GetOrCreateAdminConversationAsync(int userId);
            Task<bool> IsUserParticipantAsync(int conversationId, int userId);
            Task MarkMessageAsReadAsync(int conversationId, int userId, int messageId);
        }
    }
}
