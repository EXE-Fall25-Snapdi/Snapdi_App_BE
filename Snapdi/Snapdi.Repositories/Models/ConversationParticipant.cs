using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Snapdi.Repositories.Models;

[PrimaryKey("ConversationId", "UserId")]
public partial class ConversationParticipant
{
    [Key]
    [Column("ConversationID")]
    public int ConversationId { get; set; }

    [Key]
    [Column("UserID")]
    public int UserId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime JoinedAt { get; set; }

    [ForeignKey("ConversationId")]
    [InverseProperty("ConversationParticipants")]
    public virtual Conversation Conversation { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("ConversationParticipants")]
    public virtual User User { get; set; } = null!;
}
