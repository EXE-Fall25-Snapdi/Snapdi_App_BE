using System.ComponentModel.DataAnnotations;

namespace Snapdi.Services.DTOs
{
    public class KeywordDto
    {
        public int KeywordId { get; set; }
        public string Keyword { get; set; } = string.Empty;
        public int BlogCount { get; set; }
    }

    public class CreateKeywordDto
    {
        [Required]
        [StringLength(255, ErrorMessage = "Keyword cannot exceed 255 characters")]
        public string Keyword { get; set; } = string.Empty;
    }

    public class UpdateKeywordDto
    {
        [Required]
        [StringLength(255, ErrorMessage = "Keyword cannot exceed 255 characters")]
        public string Keyword { get; set; } = string.Empty;
    }

    public class KeywordWithBlogsDto : KeywordDto
    {
        public List<BlogSummaryDto> Blogs { get; set; } = new List<BlogSummaryDto>();
    }

    public class BlogSummaryDto
    {
        public int BlogId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string ThumbnailUrl { get; set; } = string.Empty;
        public DateTime CreateAt { get; set; }
        public bool IsActive { get; set; }
        public string? AuthorName { get; set; }
    }
}