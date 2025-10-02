using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Snapdi.Repositories.Models;

[Table("Keyword")]
public partial class Keyword
{
    [Key]
    [Column("KeywordID")]
    public int KeywordId { get; set; }

    [Column("Keyword")]
    [StringLength(255)]
    public string Keyword1 { get; set; } = null!;

    [ForeignKey("KeywordId")]
    [InverseProperty("Keywords")]
    public virtual ICollection<Blog> Blogs { get; set; } = new List<Blog>();
}
