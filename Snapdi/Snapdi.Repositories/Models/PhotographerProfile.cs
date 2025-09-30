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

    [Column("EquipmentID")]
    public int? EquipmentId { get; set; }

    public int? YearsOfExperience { get; set; }

    public double? AvgRating { get; set; }

    public bool IsAvailable { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    [Column("PhotoPortfolioID")]
    public int? PhotoPortfolioId { get; set; }

    [ForeignKey("EquipmentId")]
    [InverseProperty("PhotographerProfiles")]
    public virtual PhotoEquipment? Equipment { get; set; }

    [ForeignKey("PhotoPortfolioId")]
    [InverseProperty("PhotographerProfiles")]
    public virtual PhotoPortfolio? PhotoPortfolio { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("PhotographerProfile")]
    public virtual User User { get; set; } = null!;
}
