using System.ComponentModel.DataAnnotations;

namespace Snapdi.Services.DTOs
{
    /// <summary>
    /// DTO for finding snappers (photographers) with simple filters
    /// </summary>
    public class FindSnapperDto
    {
        /// <summary>
        /// Filter by location city (optional)
        /// </summary>
        [MaxLength(100, ErrorMessage = "City cannot exceed 100 characters")]
        public string? City { get; set; }

        /// <summary>
        /// Filter by photographer level (optional)
        /// Example values: "Beginner", "Intermediate", "Professional", "Expert"
        /// </summary>
        [MaxLength(50, ErrorMessage = "Level cannot exceed 50 characters")]
        public string? Level { get; set; }

        /// <summary>
        /// Filter by photography style IDs (optional)
        /// Example: [1, 2, 3] for specific styles
        /// </summary>
        public List<int>? StyleIds { get; set; }

        /// <summary>
        /// Filter by availability status (optional)
        /// If null, returns both available and unavailable photographers
        /// </summary>
        public bool? IsAvailable { get; set; }

        /// <summary>
        /// Page number (starts from 1)
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "Page number must be greater than 0")]
        public int Page { get; set; } = 1;

        /// <summary>
        /// Number of items per page (max 50)
        /// </summary>
        [Range(1, 50, ErrorMessage = "Page size must be between 1 and 50")]
        public int PageSize { get; set; } = 10;

        /// <summary>
        /// Sort by field: "name", "rating", "city", "level"
        /// </summary>
        [RegularExpression(@"^(name|rating|city|level)$", 
            ErrorMessage = "Sort by must be 'name', 'rating', 'city', or 'level'")]
        public string? SortBy { get; set; } = "rating";

        /// <summary>
        /// Sort direction: "asc" or "desc"
        /// </summary>
        [RegularExpression(@"^(asc|desc)$", ErrorMessage = "Sort direction must be 'asc' or 'desc'")]
        public string? SortDirection { get; set; } = "desc";
    }

    /// <summary>
    /// Response DTO for finding snappers
    /// </summary>
    public class FindSnapperResultDto
    {
        /// <summary>
        /// List of snappers matching the criteria
        /// </summary>
        public List<SnapperDto> Snappers { get; set; } = new();

        /// <summary>
        /// Total number of snappers found
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Current page number
        /// </summary>
        public int CurrentPage { get; set; }

        /// <summary>
        /// Number of items per page
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Total number of pages
        /// </summary>
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

        /// <summary>
        /// Whether there is a next page
        /// </summary>
        public bool HasNextPage => CurrentPage < TotalPages;

        /// <summary>
        /// Whether there is a previous page
        /// </summary>
        public bool HasPreviousPage => CurrentPage > 1;

        /// <summary>
        /// Number of available snappers in results
        /// </summary>
        public int AvailableCount { get; set; }

        /// <summary>
        /// Summary message
        /// </summary>
        public string Summary => $"Found {TotalCount} snapper(s), {AvailableCount} available";
    }

    /// <summary>
    /// Simplified DTO for snapper information
    /// </summary>
    public class SnapperDto
    {
        public int UserId { get; set; }
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? Phone { get; set; }
        public string? LocationCity { get; set; }
        public string? LocationAddress { get; set; }
        public string? AvatarUrl { get; set; }
        public bool IsActive { get; set; }
        public bool IsVerify { get; set; }

        // Photographer Profile Information
        public string? LevelPhotographer { get; set; }
        public bool IsAvailable { get; set; }
        public double? AvgRating { get; set; }
        public string? YearsOfExperience { get; set; }
        public string? EquipmentDescription { get; set; }
        public string? Description { get; set; }
        public double? PhotoPrice { get; set; }
        public string? PhotoType { get; set; }
        public string? WorkLocation { get; set; }

        // Style Information
        public List<StyleDto>? Styles { get; set; }

        // Additional Info
        public int PortfolioCount { get; set; }
        public List<string>? PortfolioUrls { get; set; }
    }

    /// <summary>
    /// DTO for photography style information
    /// </summary>
    public class StyleDto
    {
        public int StyleId { get; set; }
        public string StyleName { get; set; } = null!;
    }
}
