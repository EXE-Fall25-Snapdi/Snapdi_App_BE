using Microsoft.EntityFrameworkCore;
using Snapdi.Repositories.Context;
using Snapdi.Repositories.Interfaces;
using Snapdi.Repositories.Models;

namespace Snapdi.Repositories.Repositories
{
    public class MessageRepository : IMessageRepository
    {
        private readonly SnapdiDbV2Context _context;

        public MessageRepository(SnapdiDbV2Context context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Message>> GetConversationMessagesAsync(int conversationId, int? beforeMessageId = null, int take = 50)
        {
            var query = _context.Messages.Where(m => m.ConversationId == conversationId);

            if (beforeMessageId.HasValue)
            {
                query = query.Where(m => m.MessageId < beforeMessageId.Value);
            }

            var messages = await query
                .OrderByDescending(m => m.MessageId)
                .Take(take)
                .ToListAsync();

            messages.Reverse();
            return messages;
        }

        public async Task<Message> CreateMessageAsync(int conversationId, int senderId, string content)
        {
            var message = new Message
            {
                ConversationId = conversationId,
                SenderId = senderId,
                Content = content,
                SendAt = DateTime.UtcNow,
                Status = "sent"
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();
            return message;
        }

        public async Task<Message?> GetMessageByIdAsync(int messageId)
        {
            return await _context.Messages
                .Include(m => m.Sender)
                .FirstOrDefaultAsync(m => m.MessageId == messageId);
        }

        public async Task<int> GetUnreadCountAsync(int conversationId, int userId, int? lastReadMessageId = null)
        {
            var query = _context.Messages.Where(m => m.ConversationId == conversationId);

            if (lastReadMessageId.HasValue)
            {
                query = query.Where(m => m.MessageId > lastReadMessageId.Value);
            }

            return await query.CountAsync();
        }

        public async Task<Message?> GetLastMessageAsync(int conversationId)
        {
            return await _context.Messages
                .Where(m => m.ConversationId == conversationId)
                .OrderByDescending(m => m.SendAt)
                .FirstOrDefaultAsync();
        }
    }
}