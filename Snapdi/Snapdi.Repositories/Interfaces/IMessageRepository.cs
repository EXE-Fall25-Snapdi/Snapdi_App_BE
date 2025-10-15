using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Snapdi.Repositories.Models;

namespace Snapdi.Repositories.Interfaces
{
    public interface IMessageRepository
    {
        Task<IEnumerable<Message>> GetConversationMessagesAsync(int conversationId, int? beforeMessageId = null, int take = 50);
        Task<Message> CreateMessageAsync(int conversationId, int senderId, string content);
        Task<Message?> GetMessageByIdAsync(int messageId);
        Task<int> GetUnreadCountAsync(int conversationId, int userId, int? lastReadMessageId = null);
        Task<Message?> GetLastMessageAsync(int conversationId);
    }
}
