using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Snapdi.Repositories.Models;

[Table("PhotoPortfolio")]
public partial class PhotoPortfolio
{
    [Key]
    [Column("PhotoPortfolioID")]
    public int PhotoPortfolioId { get; set; }

    [StringLength(500)]
    public string PhotoUrl { get; set; } = null!;

    [InverseProperty("PhotoPortfolio")]
    public virtual ICollection<PhotographerProfile> PhotographerProfiles { get; set; } = new List<PhotographerProfile>();
}
