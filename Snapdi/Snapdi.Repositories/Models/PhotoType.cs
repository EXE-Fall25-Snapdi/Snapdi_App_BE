using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Snapdi.Repositories.Models;

[Table("PhotoType")]
public partial class PhotoType
{
    [Key]
    [Column("PhotoTypeID")]
    public int PhotoTypeId { get; set; }

    [StringLength(100)]
    public string PhotoTypeName { get; set; } = null!;

    [InverseProperty("PhotoType")]
    public virtual ICollection<PhotographerPhotoType> PhotographerPhotoTypes { get; set; } = new List<PhotographerPhotoType>();
}
