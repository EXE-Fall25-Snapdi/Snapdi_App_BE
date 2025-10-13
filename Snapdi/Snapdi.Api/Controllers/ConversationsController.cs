using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Snapdi.Repositories.Context;
using Snapdi.Repositories.Models;
using System.Security.Claims;

namespace Snapdi.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ConversationsController : ControllerBase
    {
        private readonly SnapdiDbV2Context _db;

        public ConversationsController(SnapdiDbV2Context db)
        {
            _db = db;
        }

        private int GetUserId()
        {
            var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(id)) throw new UnauthorizedAccessException();
            return int.Parse(id);
        }

        [HttpGet]
        public async Task<ActionResult> GetMyConversations()
        {
            var userId = GetUserId();
            var q = from cp in _db.ConversationParticipants
                    where cp.UserId == userId
                    join c in _db.Conversations on cp.ConversationId equals c.ConversationId
                    select new
                    {
                        c.ConversationId,
                        c.Type,
                        c.CreateAt,
                        cp.LastReadMessageId,
                        UnreadCount = _db.Messages
                            .Where(m => m.ConversationId == c.ConversationId && (cp.LastReadMessageId == null || m.MessageId > cp.LastReadMessageId))
                            .Count()
                    };
            var data = await q.OrderByDescending(x => x.CreateAt).ToListAsync();
            return Ok(data);
        }

        [HttpGet("{id}/messages")]
        public async Task<ActionResult> GetMessages(int id, [FromQuery] int? beforeMessageId, [FromQuery] int take = 50)
        {
            var userId = GetUserId();
            var isParticipant = await _db.ConversationParticipants.AnyAsync(cp => cp.ConversationId == id && cp.UserId == userId);
            if (!isParticipant) return Forbid();

            var q = _db.Messages.Where(m => m.ConversationId == id);
            if (beforeMessageId.HasValue)
            {
                q = q.Where(m => m.MessageId < beforeMessageId.Value);
            }
            var data = await q.OrderByDescending(m => m.MessageId).Take(Math.Clamp(take, 1, 200)).ToListAsync();
            data.Reverse();
            return Ok(data.Select(m => new
            {
                m.MessageId,
                m.ConversationId,
                m.SenderId,
                m.Content,
                m.SendAt
            }));
        }

        public class SendMessageRequest
        {
            public string Content { get; set; } = string.Empty;
        }

        [HttpPost("{id}/messages")]
        public async Task<ActionResult> SendMessage(int id, [FromBody] SendMessageRequest request, [FromServices] Microsoft.AspNetCore.SignalR.IHubContext<Snapdi.Api.Hubs.ChatHub> hub)
        {
            var userId = GetUserId();
            if (string.IsNullOrWhiteSpace(request.Content) || request.Content.Length > 2000)
                return BadRequest("Invalid content");

            var isParticipant = await _db.ConversationParticipants.AnyAsync(cp => cp.ConversationId == id && cp.UserId == userId);
            if (!isParticipant) return Forbid();

            var message = new Message
            {
                ConversationId = id,
                SenderId = userId,
                Content = request.Content.Trim(),
                SendAt = DateTime.UtcNow,
                Status = "sent"
            };

            _db.Messages.Add(message);
            await _db.SaveChangesAsync();

            await hub.Clients.Group($"conversation:{id}").SendCoreAsync("messageReceived", new object[] { new
            {
                messageId = message.MessageId,
                conversationId = id,
                senderId = userId,
                content = message.Content,
                sendAt = message.SendAt
            }});

            return Ok(new
            {
                message.MessageId,
                message.ConversationId,
                message.SenderId,
                message.Content,
                message.SendAt
            });
        }
        [HttpPost("admin")]
        public async Task<ActionResult> GetOrCreateAdminConversation()
        {
            var userId = GetUserId();

            // Find admin user
            var adminUser = await _db.Users
                .Where(u => u.Role.RoleName == "ADMIN")
                .FirstOrDefaultAsync();

            if (adminUser == null)
                return BadRequest("No admin found");

            // Check for existing conversation
            var existingConversation = await (
                from cp1 in _db.ConversationParticipants
                where cp1.UserId == userId
                join cp2 in _db.ConversationParticipants on cp1.ConversationId equals cp2.ConversationId
                where cp2.UserId == adminUser.UserId
                join c in _db.Conversations on cp1.ConversationId equals c.ConversationId
                where c.Type == "support"
                select c.ConversationId
            ).FirstOrDefaultAsync();

            if (existingConversation != 0)
            {
                return Ok(new { conversationId = existingConversation });
            }

            // Create new conversation
            var conversation = new Conversation
            {
                Type = "support",
                CreateAt = DateTime.UtcNow
            };

            _db.Conversations.Add(conversation);
            await _db.SaveChangesAsync();

            // Add participants
            _db.ConversationParticipants.AddRange(
                new ConversationParticipant
                {
                    ConversationId = conversation.ConversationId,
                    UserId = userId,
                    JoinedAt = DateTime.UtcNow
                },
                new ConversationParticipant
                {
                    ConversationId = conversation.ConversationId,
                    UserId = adminUser.UserId,
                    JoinedAt = DateTime.UtcNow
                }
            );

            await _db.SaveChangesAsync();

            return Ok(new { conversationId = conversation.ConversationId });
        }
    }
}


