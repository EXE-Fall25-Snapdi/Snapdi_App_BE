using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snapdi.Services.DTOs
{
    public class ConversationDto
    {
        public int ConversationId { get; set; }
        public string Type { get; set; } = string.Empty;
        public DateTime CreateAt { get; set; }
        public int? LastReadMessageId { get; set; }
        public int UnreadCount { get; set; }
        public string? LastMessageContent { get; set; }
        public DateTime? LastMessageTime { get; set; }
        public List<ConversationParticipantDto> Participants { get; set; } = new();
    }

    public class ConversationParticipantDto
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? Avatar { get; set; }
        public DateTime JoinedAt { get; set; }
    }
}