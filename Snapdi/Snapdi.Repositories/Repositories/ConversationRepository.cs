using Microsoft.EntityFrameworkCore;
using Snapdi.Repositories.Context;
using Snapdi.Repositories.Interfaces;
using Snapdi.Repositories.Models;

namespace Snapdi.Repositories.Repositories
{
    public class ConversationRepository : IConversationRepository
    {
        private readonly SnapdiDbV2Context _context;

        public ConversationRepository(SnapdiDbV2Context context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Conversation>> GetUserConversationsAsync(int userId)
        {
            return await _context.ConversationParticipants
                .Where(cp => cp.UserId == userId)
                .Include(cp => cp.Conversation)
                .Select(cp => cp.Conversation)
                .OrderByDescending(c => c.CreateAt)
                .ToListAsync();
        }

        public async Task<Conversation?> GetConversationByIdAsync(int conversationId)
        {
            return await _context.Conversations
                .Include(c => c.ConversationParticipants)
                .ThenInclude(cp => cp.User)
                .FirstOrDefaultAsync(c => c.ConversationId == conversationId);
        }

        public async Task<bool> IsUserParticipantAsync(int conversationId, int userId)
        {
            return await _context.ConversationParticipants
                .AnyAsync(cp => cp.ConversationId == conversationId && cp.UserId == userId);
        }

        public async Task<int?> GetSupportConversationAsync(int userId, int adminUserId)
        {
            var conversationId = await (
                from cp1 in _context.ConversationParticipants
                where cp1.UserId == userId
                join cp2 in _context.ConversationParticipants on cp1.ConversationId equals cp2.ConversationId
                where cp2.UserId == adminUserId
                join c in _context.Conversations on cp1.ConversationId equals c.ConversationId
                where c.Type == "support"
                select c.ConversationId
            ).FirstOrDefaultAsync();

            return conversationId == 0 ? null : conversationId;
        }

        public async Task<int> CreateSupportConversationAsync(int userId, int adminUserId)
        {
            var conversation = new Conversation
            {
                Type = "support",
                CreateAt = DateTime.UtcNow
            };

            _context.Conversations.Add(conversation);
            await _context.SaveChangesAsync();

            _context.ConversationParticipants.AddRange(
                new ConversationParticipant
                {
                    ConversationId = conversation.ConversationId,
                    UserId = userId,
                    JoinedAt = DateTime.UtcNow
                },
                new ConversationParticipant
                {
                    ConversationId = conversation.ConversationId,
                    UserId = adminUserId,
                    JoinedAt = DateTime.UtcNow
                }
            );

            await _context.SaveChangesAsync();
            return conversation.ConversationId;
        }

        public async Task UpdateLastReadMessageAsync(int conversationId, int userId, int messageId)
        {
            var participant = await _context.ConversationParticipants
                .FirstOrDefaultAsync(cp => cp.ConversationId == conversationId && cp.UserId == userId);

            if (participant != null)
            {
                participant.LastReadMessageId = messageId;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<ConversationParticipant>> GetConversationParticipantsAsync(int conversationId)
        {
            return await _context.ConversationParticipants
                .Where(cp => cp.ConversationId == conversationId)
                .Include(cp => cp.User)
                .ToListAsync();
        }
    }
}