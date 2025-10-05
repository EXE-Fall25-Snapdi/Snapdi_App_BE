using System.ComponentModel.DataAnnotations;

namespace Snapdi.Services.DTOs
{
    /// <summary>
    /// DTO for filtering and paging users - all filter fields are optional (can be null)
    /// </summary>
    public class UserFilterDto
    {
        /// <summary>
        /// Page number (1-based)
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "Page must be greater than 0")]
        public int Page { get; set; } = 1;

        /// <summary>
        /// Number of items per page (max 100)
        /// </summary>
        [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100")]
        public int PageSize { get; set; } = 10;

        /// <summary>
        /// Search term for name or email (optional)
        /// </summary>
        public string? SearchTerm { get; set; }

        /// <summary>
        /// Filter by role ID (optional)
        /// </summary>
        public int? RoleId { get; set; }

        /// <summary>
        /// Filter by active status (optional - null means include both active and inactive)
        /// </summary>
        public bool? IsActive { get; set; }

        /// <summary>
        /// Filter by verification status (optional - null means include both verified and unverified)
        /// </summary>
        public bool? IsVerified { get; set; }

        /// <summary>
        /// Filter by location city (optional)
        /// </summary>
        public string? LocationCity { get; set; }

        /// <summary>
        /// Sort field (optional - valid values: "name", "email", "createdAt")
        /// </summary>
        public string? SortBy { get; set; }

        /// <summary>
        /// Sort direction (optional - valid values: "asc", "desc", defaults to "asc" if null)
        /// </summary>
        public string? SortDirection { get; set; }

        /// <summary>
        /// Filter by date range - from date (optional)
        /// </summary>
        public DateTime? CreatedFrom { get; set; }

        /// <summary>
        /// Filter by date range - to date (optional)
        /// </summary>
        public DateTime? CreatedTo { get; set; }
    }
}