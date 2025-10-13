using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Snapdi.Repositories.Models;

[Table("Booking")]
public partial class Booking
{
    [Key]
    [Column("BookingID")]
    public int BookingId { get; set; }

    [Column("CustomerID")]
    public int? CustomerId { get; set; }

    [Column("PhotographerID")]
    public int? PhotographerId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime ScheduleAt { get; set; }

    [StringLength(100)]
    public string? LocationCity { get; set; }

    [StringLength(255)]
    public string? LocationAddress { get; set; }

    [Column("StyleID")]
    public int? StyleId { get; set; }

    [Column("StatusID")]
    public int? StatusId { get; set; }

    public double Price { get; set; }

    [ForeignKey("CustomerId")]
    [InverseProperty("BookingCustomers")]
    public virtual User? Customer { get; set; }

    [InverseProperty("Booking")]
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    [ForeignKey("PhotographerId")]
    [InverseProperty("BookingPhotographers")]
    public virtual User? Photographer { get; set; }

    [InverseProperty("Booking")]
    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    [ForeignKey("StatusId")]
    [InverseProperty("Bookings")]
    public virtual BookingStatus? Status { get; set; }

    [ForeignKey("StyleId")]
    [InverseProperty("Bookings")]
    public virtual Style? Style { get; set; }

    [InverseProperty("Booking")]
    public virtual ICollection<VoucherUsage> VoucherUsages { get; set; } = new List<VoucherUsage>();
}
