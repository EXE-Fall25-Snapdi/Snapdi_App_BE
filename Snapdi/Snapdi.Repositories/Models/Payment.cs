using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Snapdi.Repositories.Models;

[Table("Payment")]
public partial class Payment
{
    [Key]
    [Column("PaymentID")]
    public int PaymentId { get; set; }

    [Column("BookingID")]
    public int? BookingId { get; set; }

    public double Amount { get; set; }

    [Column("FeePolicyID")]
    public int? FeePolicyId { get; set; }

    public double? FeePercent { get; set; }

    public double? FeeAmount { get; set; }

    public double? NetAmount { get; set; }

    [StringLength(50)]
    public string? TransactionMethod { get; set; }

    [StringLength(255)]
    public string? TransactionReference { get; set; }

    [Column("PaymentStatusID")]
    public int? PaymentStatusId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime PaymentDate { get; set; }

    [ForeignKey("BookingId")]
    [InverseProperty("Payments")]
    public virtual Booking? Booking { get; set; }

    [ForeignKey("FeePolicyId")]
    [InverseProperty("Payments")]
    public virtual FeePolicy? FeePolicy { get; set; }

    [ForeignKey("PaymentStatusId")]
    [InverseProperty("Payments")]
    public virtual PaymentStatus? PaymentStatus { get; set; }
}
