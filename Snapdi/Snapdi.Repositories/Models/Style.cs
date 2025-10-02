using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Snapdi.Repositories.Models;

public partial class Style
{
    [Key]
    [Column("StyleID")]
    public int StyleId { get; set; }

    [StringLength(100)]
    public string StyleName { get; set; } = null!;

    [InverseProperty("Style")]
    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
