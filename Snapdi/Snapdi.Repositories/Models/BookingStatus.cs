using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Snapdi.Repositories.Models;

[Table("BookingStatus")]
public partial class BookingStatus
{
    [Key]
    [Column("StatusID")]
    public int StatusId { get; set; }

    [StringLength(100)]
    public string StatusName { get; set; } = null!;

    [InverseProperty("Status")]
    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
