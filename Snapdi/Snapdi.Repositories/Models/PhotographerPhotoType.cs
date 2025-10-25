using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Snapdi.Repositories.Models;

[Table("PhotographerPhotoType")]
[PrimaryKey(nameof(UserId), nameof(PhotoTypeId))]
public partial class PhotographerPhotoType
{
    [Key]
    [Column("UserID")]
    public int UserId { get; set; }

    [Key]
    [Column("PhotoTypeID")]
    public int PhotoTypeId { get; set; }

    public double? PhotoPrice { get; set; }

    public int? Time { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("PhotographerPhotoTypes")]
    public virtual PhotographerProfile PhotographerProfile { get; set; } = null!;

    [ForeignKey("PhotoTypeId")]
    [InverseProperty("PhotographerPhotoTypes")]
    public virtual PhotoType PhotoType { get; set; } = null!;
}
