using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Snapdi.Repositories.Context;
using Snapdi.Repositories.Models;
using System.Security.Claims;

namespace Snapdi.Api.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly SnapdiDbV2Context _db;

        public ChatHub(SnapdiDbV2Context db)
        {
            _db = db;
        }

        private int GetUserId()
        {
            var id = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(id)) throw new HubException("Unauthorized");
            return int.Parse(id);
        }

        private static string GroupName(int conversationId) => $"conversation:{conversationId}";

        public async Task JoinConversation(int conversationId)
        {
            var userId = GetUserId();
            var isParticipant = await _db.ConversationParticipants
                .AnyAsync(cp => cp.ConversationId == conversationId && cp.UserId == userId);
            if (!isParticipant)
                throw new HubException("Not a participant of this conversation");
            await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(conversationId));
        }

        public async Task LeaveConversation(int conversationId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(conversationId));
        }

        public async Task SendMessage(int conversationId, string content)
        {
            var userId = GetUserId();

            if (string.IsNullOrWhiteSpace(content) || content.Length > 2000)
                throw new HubException("Invalid content");

            var isParticipant = await _db.ConversationParticipants
                .AnyAsync(cp => cp.ConversationId == conversationId && cp.UserId == userId);
            if (!isParticipant)
                throw new HubException("Not a participant of this conversation");

            var message = new Message
            {
                ConversationId = conversationId,
                SenderId = userId,
                Content = content.Trim(),
                SendAt = DateTime.UtcNow,
                Status = "sent"
            };

            _db.Messages.Add(message);
            await _db.SaveChangesAsync();

            await Clients.Group(GroupName(conversationId)).SendAsync("messageReceived", new
            {
                messageId = message.MessageId,
                conversationId,
                senderId = userId,
                content = message.Content,
                sendAt = message.SendAt
            });
        }

        public async Task MarkRead(int conversationId, int messageId)
        {
            var userId = GetUserId();

            var isParticipant = await _db.ConversationParticipants
                .AnyAsync(cp => cp.ConversationId == conversationId && cp.UserId == userId);
            if (!isParticipant)
                throw new HubException("Not a participant of this conversation");

            var message = await _db.Messages
                .Where(m => m.ConversationId == conversationId && m.MessageId == messageId)
                .FirstOrDefaultAsync();
            if (message == null) throw new HubException("Message not found");

            // Option A: store last read in participant (we will add fields via migration)
            var participant = await _db.ConversationParticipants
                .FirstAsync(cp => cp.ConversationId == conversationId && cp.UserId == userId);

            // If fields exist, update them; if not yet migrated, skip silently
            var participantType = participant.GetType();
            var lastReadMsgProp = participantType.GetProperty("LastReadMessageId");
            var lastReadAtProp = participantType.GetProperty("LastReadAt");
            if (lastReadMsgProp != null && lastReadAtProp != null)
            {
                var current = (int?)lastReadMsgProp.GetValue(participant);
                if (!current.HasValue || messageId > current.Value)
                {
                    lastReadMsgProp.SetValue(participant, messageId);
                    lastReadAtProp.SetValue(participant, DateTime.UtcNow);
                    await _db.SaveChangesAsync();
                }
            }

            await Clients.Group(GroupName(conversationId)).SendAsync("messageRead", new
            {
                conversationId,
                messageId,
                readerUserId = userId,
                readAt = DateTime.UtcNow
            });
        }
    }
}


