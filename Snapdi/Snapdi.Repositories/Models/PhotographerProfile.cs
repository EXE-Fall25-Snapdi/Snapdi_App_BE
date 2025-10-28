using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Snapdi.Repositories.Models;

[Table("PhotographerProfile")]
public partial class PhotographerProfile
{
    [Key]
    [Column("UserID")]
    public int UserId { get; set; }

    [StringLength(500)]
    public string? EquipmentDescription { get; set; }

    [StringLength(100)]
    public string? YearsOfExperience { get; set; }

    public double? AvgRating { get; set; }

    public bool IsAvailable { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    [StringLength(50)]
    public string? LevelPhotographer { get; set; }

    [StringLength(255)]
    public string? WorkLocation { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("PhotographerProfile")]
    public virtual User User { get; set; } = null!;

    [InverseProperty("PhotographerProfile")]
    public virtual ICollection<PhotographerStyle> PhotographerStyles { get; set; } = new List<PhotographerStyle>();

    [InverseProperty("PhotographerProfile")]
    public virtual ICollection<PhotographerPhotoType> PhotographerPhotoTypes { get; set; } = new List<PhotographerPhotoType>();
}
