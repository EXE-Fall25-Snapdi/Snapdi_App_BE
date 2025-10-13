using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Snapdi.Repositories.Models;

[Table("PaymentStatus")]
public partial class PaymentStatus
{
    [Key]
    [Column("PaymentStatusID")]
    public int PaymentStatusId { get; set; }

    [StringLength(100)]
    public string StatusName { get; set; } = null!;

    [InverseProperty("PaymentStatus")]
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
