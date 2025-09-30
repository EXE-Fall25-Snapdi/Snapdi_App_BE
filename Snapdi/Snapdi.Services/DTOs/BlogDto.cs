using System.ComponentModel.DataAnnotations;

namespace Snapdi.Services.DTOs
{
    public class BlogDto
    {
        public int BlogId { get; set; }
        public int? AuthorId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string ThumbnailUrl { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreateAt { get; set; }
        public DateTime? UpdateAt { get; set; }
        public bool IsActive { get; set; }
        public string? AuthorName { get; set; }
        public List<KeywordDto> Keywords { get; set; } = new List<KeywordDto>();
    }

    public class CreateBlogDto
    {
        [Required]
        [StringLength(255, ErrorMessage = "Title cannot exceed 255 characters")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(255, ErrorMessage = "Thumbnail URL cannot exceed 255 characters")]
        public string ThumbnailUrl { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        public int? AuthorId { get; set; }

        public bool IsActive { get; set; } = true;

        public List<string> KeywordNames { get; set; } = new List<string>();
        public List<int> KeywordIds { get; set; } = new List<int>();
    }

    public class UpdateBlogDto
    {
        [Required]
        [StringLength(255, ErrorMessage = "Title cannot exceed 255 characters")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(255, ErrorMessage = "Thumbnail URL cannot exceed 255 characters")]
        public string ThumbnailUrl { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public List<string> KeywordNames { get; set; } = new List<string>();
        public List<int> KeywordIds { get; set; } = new List<int>();
    }

    // Paging DTOs
    public class PagedRequest
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class PagedResult<T>
    {
        public List<T> Data { get; set; } = new List<T>();
        public int TotalRecords { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalRecords / PageSize);
        public bool HasNextPage => PageNumber < TotalPages;
        public bool HasPreviousPage => PageNumber > 1;
    }
}