using Snapdi.Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snapdi.Repositories.Interfaces
{
    public interface IConversationRepository
    {
        Task<IEnumerable<Conversation>> GetUserConversationsAsync(int userId);
        Task<Conversation?> GetConversationByIdAsync(int conversationId);
        Task<bool> IsUserParticipantAsync(int conversationId, int userId);
        Task<int?> GetSupportConversationAsync(int userId, int adminUserId);
        Task<int> CreateSupportConversationAsync(int userId, int adminUserId);
        Task UpdateLastReadMessageAsync(int conversationId, int userId, int messageId);
        Task<IEnumerable<ConversationParticipant>> GetConversationParticipantsAsync(int conversationId);
    }
}
