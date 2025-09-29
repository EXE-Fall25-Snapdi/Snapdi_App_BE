using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Snapdi.Repositories.Models;

[Table("Voucher")]
[Index("Code", Name = "UQ__Voucher__A25C5AA76781C3B2", IsUnique = true)]
public partial class Voucher
{
    [Key]
    [Column("VoucherID")]
    public int VoucherId { get; set; }

    [StringLength(50)]
    public string Code { get; set; } = null!;

    [StringLength(255)]
    public string? Description { get; set; }

    [StringLength(50)]
    public string? DiscountType { get; set; }

    public double DiscountValue { get; set; }

    public double? MaxDiscount { get; set; }

    public double? MinSpend { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime StartDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime EndDate { get; set; }

    public int? UsageLimit { get; set; }

    public bool IsActive { get; set; }

    [InverseProperty("Voucher")]
    public virtual ICollection<VoucherUsage> VoucherUsages { get; set; } = new List<VoucherUsage>();
}
