using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Snapdi.Repositories.Models;

[Table("FeePolicy")]
public partial class FeePolicy
{
    [Key]
    [Column("FeePolicyID")]
    public int FeePolicyId { get; set; }

    [StringLength(100)]
    public string? TransactionType { get; set; }

    public double FeePercent { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime EffectiveDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ExpiryDate { get; set; }

    public bool IsActive { get; set; }

    [InverseProperty("FeePolicy")]
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
