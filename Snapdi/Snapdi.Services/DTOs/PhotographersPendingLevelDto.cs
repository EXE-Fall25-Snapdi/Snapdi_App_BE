using System.ComponentModel.DataAnnotations;

namespace Snapdi.Services.DTOs
{
    /// <summary>
    /// Request DTO for getting photographers pending level assignment with paging and filtering
    /// </summary>
    public class GetPhotographersPendingLevelRequestDto
    {
        /// <summary>
        /// Page number (starts from 1)
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "Page must be greater than 0")]
        public int Page { get; set; } = 1;

        /// <summary>
        /// Number of items per page (max 50)
        /// </summary>
        [Range(1, 50, ErrorMessage = "Page size must be between 1 and 50")]
        public int PageSize { get; set; } = 10;

        /// <summary>
        /// Search term for photographer name or email
        /// </summary>
        [MaxLength(255, ErrorMessage = "Search term cannot exceed 255 characters")]
        public string? SearchTerm { get; set; }

        /// <summary>
        /// Filter by portfolio status: null = all, true = with portfolio, false = without portfolio
        /// </summary>
        public bool? HasPortfolio { get; set; }

        /// <summary>
        /// Location city filter
        /// </summary>
        [MaxLength(100, ErrorMessage = "Location city cannot exceed 100 characters")]
        public string? LocationCity { get; set; }

        /// <summary>
        /// Sort by field: "name", "email", "createdAt"
        /// </summary>
        [RegularExpression(@"^(name|email|createdAt)$", ErrorMessage = "Sort by must be 'name', 'email', or 'createdAt'")]
        public string? SortBy { get; set; } = "createdAt";

        /// <summary>
        /// Sort direction: "asc" or "desc"
        /// </summary>
        [RegularExpression(@"^(asc|desc)$", ErrorMessage = "Sort direction must be 'asc' or 'desc'")]
        public string? SortDirection { get; set; } = "desc";

        /// <summary>
        /// Created from date filter
        /// </summary>
        public DateTime? CreatedFrom { get; set; }

        /// <summary>
        /// Created to date filter
        /// </summary>
        public DateTime? CreatedTo { get; set; }
    }

    /// <summary>
    /// Response DTO for photographers pending level assignment with paging
    /// </summary>
    public class PhotograhpersPendingLevelPagedResponseDto
    {
        /// <summary>
        /// Photographers who have portfolio photos uploaded
        /// </summary>
        public PagedResultDto<UserWithPhotographerDto> WithPortfolio { get; set; } = new();

        /// <summary>
        /// Photographers who don't have any portfolio photos yet
        /// </summary>
        public PagedResultDto<UserWithPhotographerDto> WithoutPortfolio { get; set; } = new();

        /// <summary>
        /// Total count of photographers pending level assignment (across both groups)
        /// </summary>
        public int TotalCount => WithPortfolio.TotalItems + WithoutPortfolio.TotalItems;

        /// <summary>
        /// Summary statistics
        /// </summary>
        public PhotographersPendingLevelSummaryDto Summary { get; set; } = new();
    }

    /// <summary>
    /// Summary statistics for photographers pending level assignment
    /// </summary>
    public class PhotographersPendingLevelSummaryDto
    {
        /// <summary>
        /// Total count of photographers with portfolio
        /// </summary>
        public int WithPortfolioCount { get; set; }

        /// <summary>
        /// Total count of photographers without portfolio
        /// </summary>
        public int WithoutPortfolioCount { get; set; }

        /// <summary>
        /// Total count of all photographers pending level assignment
        /// </summary>
        public int TotalCount => WithPortfolioCount + WithoutPortfolioCount;

        /// <summary>
        /// Percentage of photographers with portfolio
        /// </summary>
        public double WithPortfolioPercentage => TotalCount > 0 ? Math.Round((double)WithPortfolioCount / TotalCount * 100, 1) : 0;

        /// <summary>
        /// Percentage of photographers without portfolio
        /// </summary>
        public double WithoutPortfolioPercentage => TotalCount > 0 ? Math.Round((double)WithoutPortfolioCount / TotalCount * 100, 1) : 0;

        /// <summary>
        /// Summary message for admin
        /// </summary>
        public string Message => TotalCount == 0 
            ? "No photographers are currently pending level assignment."
            : $"Found {TotalCount} photographer(s) pending level assignment: {WithPortfolioCount} with portfolio ({WithPortfolioPercentage}%), {WithoutPortfolioCount} without portfolio ({WithoutPortfolioPercentage}%).";

        /// <summary>
        /// Recommendation for admin action
        /// </summary>
        public string Recommendation => TotalCount == 0 
            ? "No action needed at this time."
            : WithPortfolioCount > 0 
                ? "Recommend reviewing photographers with portfolios first as they are more ready for level assignment."
                : "All pending photographers need to upload portfolios before level assignment.";
    }

    /// <summary>
    /// Response DTO for photographers pending level assignment, grouped by portfolio status
    /// </summary>
    public class PhotograhpersPendingLevelResponseDto
    {
        /// <summary>
        /// Photographers who have portfolio photos uploaded
        /// </summary>
        public List<UserWithPhotographerDto> WithPortfolio { get; set; } = new();

        /// <summary>
        /// Photographers who don't have any portfolio photos yet
        /// </summary>
        public List<UserWithPhotographerDto> WithoutPortfolio { get; set; } = new();

        /// <summary>
        /// Total count of photographers pending level assignment
        /// </summary>
        public int TotalCount => WithPortfolio.Count + WithoutPortfolio.Count;

        /// <summary>
        /// Count of photographers with portfolio
        /// </summary>
        public int WithPortfolioCount => WithPortfolio.Count;

        /// <summary>
        /// Count of photographers without portfolio
        /// </summary>
        public int WithoutPortfolioCount => WithoutPortfolio.Count;

        /// <summary>
        /// Percentage of photographers with portfolio
        /// </summary>
        public double WithPortfolioPercentage => TotalCount > 0 ? Math.Round((double)WithPortfolioCount / TotalCount * 100, 1) : 0;

        /// <summary>
        /// Percentage of photographers without portfolio
        /// </summary>
        public double WithoutPortfolioPercentage => TotalCount > 0 ? Math.Round((double)WithoutPortfolioCount / TotalCount * 100, 1) : 0;

        /// <summary>
        /// Summary message for admin
        /// </summary>
        public string Summary => TotalCount == 0 
            ? "No photographers are currently pending level assignment."
            : $"Found {TotalCount} photographer(s) pending level assignment: {WithPortfolioCount} with portfolio ({WithPortfolioPercentage}%), {WithoutPortfolioCount} without portfolio ({WithoutPortfolioPercentage}%).";

        /// <summary>
        /// Recommendation for admin action
        /// </summary>
        public string Recommendation => TotalCount == 0 
            ? "No action needed at this time."
            : WithPortfolioCount > 0 
                ? "Recommend reviewing photographers with portfolios first as they are more ready for level assignment."
                : "All pending photographers need to upload portfolios before level assignment.";
    }
}