using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Snapdi.Repositories.Models;

[Table("Review")]
public partial class Review
{
    [Key]
    [Column("ReviewID")]
    public int ReviewId { get; set; }

    [Column("BookingID")]
    public int? BookingId { get; set; }

    [Column("FromUserID")]
    public int? FromUserId { get; set; }

    [Column("ToUserID")]
    public int? ToUserId { get; set; }

    public double Rating { get; set; }

    public string? Comment { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreateAt { get; set; }

    [ForeignKey("BookingId")]
    [InverseProperty("Reviews")]
    public virtual Booking? Booking { get; set; }

    [ForeignKey("FromUserId")]
    [InverseProperty("ReviewFromUsers")]
    public virtual User? FromUser { get; set; }

    [ForeignKey("ToUserId")]
    [InverseProperty("ReviewToUsers")]
    public virtual User? ToUser { get; set; }
}
