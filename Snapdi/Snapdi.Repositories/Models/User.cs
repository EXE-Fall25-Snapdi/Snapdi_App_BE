using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Snapdi.Repositories.Models;

[Table("User")]
[Index("Email", Name = "UQ__User__A9D10534DB5CBBA4", IsUnique = true)]
public partial class User
{
    [Key]
    [Column("UserID")]
    public int UserId { get; set; }

    [Column("RoleID")]
    public int? RoleId { get; set; }

    [StringLength(255)]
    public string Name { get; set; } = null!;

    [StringLength(255)]
    public string Email { get; set; } = null!;

    [StringLength(50)]
    public string? Phone { get; set; }

    [StringLength(255)]
    public string Password { get; set; } = null!;

    [StringLength(255)]
    public string? RefreshToken { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ExpiredRefreshTokenAt { get; set; }

    public bool IsActive { get; set; }

    public bool IsVerify { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreatedAt { get; set; }

    [StringLength(255)]
    public string? LocationAddress { get; set; }

    [StringLength(100)]
    public string? LocationCity { get; set; }

    [StringLength(255)]
    public string? AvatarUrl { get; set; }

    [InverseProperty("Author")]
    public virtual ICollection<Blog> Blogs { get; set; } = new List<Blog>();

    [InverseProperty("Customer")]
    public virtual ICollection<Booking> BookingCustomers { get; set; } = new List<Booking>();

    [InverseProperty("Photographer")]
    public virtual ICollection<Booking> BookingPhotographers { get; set; } = new List<Booking>();

    [InverseProperty("User")]
    public virtual ICollection<ConversationParticipant> ConversationParticipants { get; set; } = new List<ConversationParticipant>();

    [InverseProperty("Sender")]
    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    [InverseProperty("User")]
    public virtual ICollection<PhotoEquipment> PhotoEquipments { get; set; } = new List<PhotoEquipment>();

    [InverseProperty("User")]
    public virtual PhotographerProfile? PhotographerProfile { get; set; }

    [InverseProperty("FromUser")]
    public virtual ICollection<Review> ReviewFromUsers { get; set; } = new List<Review>();

    [InverseProperty("ToUser")]
    public virtual ICollection<Review> ReviewToUsers { get; set; } = new List<Review>();

    [ForeignKey("RoleId")]
    [InverseProperty("Users")]
    public virtual Role? Role { get; set; }

    [InverseProperty("User")]
    public virtual ICollection<VoucherUsage> VoucherUsages { get; set; } = new List<VoucherUsage>();
}
