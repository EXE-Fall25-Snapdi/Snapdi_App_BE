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

    [ForeignKey("UserId")]
    [InverseProperty("PhotographerProfile")]
    public virtual User User { get; set; } = null!;
}
