using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Snapdi.Repositories.Models;

[Table("VoucherUsage")]
public partial class VoucherUsage
{
    [Key]
    [Column("VoucherUsageID")]
    public int VoucherUsageId { get; set; }

    [Column("BookingID")]
    public int? BookingId { get; set; }

    [Column("VoucherID")]
    public int? VoucherId { get; set; }

    [Column("UserID")]
    public int? UserId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime UsedAt { get; set; }

    [ForeignKey("BookingId")]
    [InverseProperty("VoucherUsages")]
    public virtual Booking? Booking { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("VoucherUsages")]
    public virtual User? User { get; set; }

    [ForeignKey("VoucherId")]
    [InverseProperty("VoucherUsages")]
    public virtual Voucher? Voucher { get; set; }
}
