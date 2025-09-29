using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Snapdi.Repositories.Models;

[Table("PhotoEquipment")]
public partial class PhotoEquipment
{
    [Key]
    [Column("EquipmentID")]
    public int EquipmentId { get; set; }

    [Column("UserID")]
    public int? UserId { get; set; }

    [StringLength(255)]
    public string Name { get; set; } = null!;

    [InverseProperty("Equipment")]
    public virtual ICollection<PhotographerProfile> PhotographerProfiles { get; set; } = new List<PhotographerProfile>();

    [ForeignKey("UserId")]
    [InverseProperty("PhotoEquipments")]
    public virtual User? User { get; set; }
}
