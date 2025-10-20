using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Snapdi.Repositories.Models;

[Table("PhotographerStyle")]
[PrimaryKey(nameof(UserId), nameof(StyleId))]
public partial class PhotographerStyle
{
    [Key]
    [Column("UserID")]
    public int UserId { get; set; }

    [Key]
    [Column("StyleID")]
    public int StyleId { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("PhotographerStyles")]
    public virtual PhotographerProfile PhotographerProfile { get; set; } = null!;

    [ForeignKey("StyleId")]
    [InverseProperty("PhotographerStyles")]
    public virtual Style Style { get; set; } = null!;
}
