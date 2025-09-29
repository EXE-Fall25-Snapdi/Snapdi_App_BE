using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Snapdi.Repositories.Models;

public partial class Message
{
    [Key]
    [Column("MessageID")]
    public int MessageId { get; set; }

    [Column("ConversationID")]
    public int? ConversationId { get; set; }

    [Column("SenderID")]
    public int? SenderId { get; set; }

    public string Content { get; set; } = null!;

    [Column(TypeName = "datetime")]
    public DateTime SendAt { get; set; }

    [StringLength(50)]
    public string? Status { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ExpiredDate { get; set; }

    [ForeignKey("ConversationId")]
    [InverseProperty("Messages")]
    public virtual Conversation? Conversation { get; set; }

    [ForeignKey("SenderId")]
    [InverseProperty("Messages")]
    public virtual User? Sender { get; set; }
}
