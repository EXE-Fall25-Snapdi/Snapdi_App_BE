using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Snapdi.Repositories.Models;

public partial class Conversation
{
    [Key]
    [Column("ConversationID")]
    public int ConversationId { get; set; }

    [StringLength(50)]
    public string? Type { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreateAt { get; set; }

    [InverseProperty("Conversation")]
    public virtual ICollection<ConversationParticipant> ConversationParticipants { get; set; } = new List<ConversationParticipant>();

    [InverseProperty("Conversation")]
    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
}
