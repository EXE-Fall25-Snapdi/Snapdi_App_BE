using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Snapdi.Repositories.Models;

[Table("Blog")]
public partial class Blog
{
    [Key]
    [Column("BlogID")]
    public int BlogId { get; set; }

    [Column("AuthorID")]
    public int? AuthorId { get; set; }

    [StringLength(255)]
    public string Title { get; set; } = null!;

    [StringLength(255)]
    public string ThumbnailUrl { get; set; } = null!;

    public string Content { get; set; } = null!;

    [Column(TypeName = "datetime")]
    public DateTime CreateAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? UpdateAt { get; set; }

    public bool IsActive { get; set; }

    [ForeignKey("AuthorId")]
    [InverseProperty("Blogs")]
    public virtual User? Author { get; set; }

    [ForeignKey("BlogId")]
    [InverseProperty("Blogs")]
    public virtual ICollection<Keyword> Keywords { get; set; } = new List<Keyword>();
}
