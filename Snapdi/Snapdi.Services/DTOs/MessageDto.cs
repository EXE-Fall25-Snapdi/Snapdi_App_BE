using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snapdi.Services.DTOs
{
    public class MessageDto
    {
        public int MessageId { get; set; }
        public int ConversationId { get; set; }
        public int SenderId { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime SendAt { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? SenderName { get; set; }
        public string? SenderAvatar { get; set; }
    }
}
