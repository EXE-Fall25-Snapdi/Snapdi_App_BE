using System.ComponentModel.DataAnnotations;

namespace Snapdi.Services.DTOs
{
    /// <summary>
    /// DTO for photographer search with filtering and paging
    /// </summary>
    public class PhotographerSearchDto
    {
        /// <summary>
        /// Search term for photographer name, email, or description
        /// </summary>
        public string? SearchTerm { get; set; }

        /// <summary>
        /// Filter by location city
        /// </summary>
        public string? LocationCity { get; set; }

        /// <summary>
        /// Filter by photographer level (e.g., "Beginner", "Professional", etc.)
        /// </summary>
        public string? LevelPhotographer { get; set; }

        /// <summary>
        /// Filter by availability status
        /// </summary>
        public bool? IsAvailable { get; set; }

        /// <summary>
        /// Filter by verification status
        /// </summary>
        public bool? IsVerify { get; set; }

        /// <summary>
        /// Filter by active status
        /// </summary>
        public bool? IsActive { get; set; }

        /// <summary>
        /// Minimum average rating (0.0 to 5.0)
        /// </summary>
        [Range(0.0, 5.0, ErrorMessage = "Rating must be between 0.0 and 5.0")]
        public double? MinRating { get; set; }

        /// <summary>
        /// Maximum average rating (0.0 to 5.0)
        /// </summary>
        [Range(0.0, 5.0, ErrorMessage = "Rating must be between 0.0 and 5.0")]
        public double? MaxRating { get; set; }

        /// <summary>
        /// Years of experience filter
        /// </summary>
        public string? YearsOfExperience { get; set; }

        /// <summary>
        /// Filter by photographers who have portfolios
        /// </summary>
        public bool? HasPortfolio { get; set; }

        /// <summary>
        /// Date range filter - from date
        /// </summary>
        public DateTime? CreatedFrom { get; set; }

        /// <summary>
        /// Date range filter - to date  
        /// </summary>
        public DateTime? CreatedTo { get; set; }

        /// <summary>
        /// Sort by field: "name", "email", "rating", "createdAt", "yearsOfExperience"
        /// </summary>
        [RegularExpression(@"^(name|email|rating|createdAt|yearsOfExperience)$", 
            ErrorMessage = "Sort by must be 'name', 'email', 'rating', 'createdAt', or 'yearsOfExperience'")]
        public string? SortBy { get; set; } = "createdAt";

        /// <summary>
        /// Sort direction: "asc" or "desc"
        /// </summary>
        [RegularExpression(@"^(asc|desc)$", ErrorMessage = "Sort direction must be 'asc' or 'desc'")]
        public string? SortDirection { get; set; } = "desc";

        /// <summary>
        /// Page number (starts from 1)
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "Page number must be greater than 0")]
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// Number of items per page (max 100)
        /// </summary>
        [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100")]
        public int PageSize { get; set; } = 10;
    }

    /// <summary>
    /// Response DTO for photographer search results
    /// </summary>
    public class PhotographerSearchResultDto
    {
        /// <summary>
        /// List of photographers matching the search criteria
        /// </summary>
        public List<UserWithPhotographerDto> Data { get; set; } = new();

        /// <summary>
        /// Total number of records found
        /// </summary>
        public int TotalRecords { get; set; }

        /// <summary>
        /// Current page number
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// Number of items per page
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Total number of pages
        /// </summary>
        public int TotalPages => (int)Math.Ceiling((double)TotalRecords / PageSize);

        /// <summary>
        /// Whether there is a next page
        /// </summary>
        public bool HasNextPage => PageNumber < TotalPages;

        /// <summary>
        /// Whether there is a previous page
        /// </summary>
        public bool HasPreviousPage => PageNumber > 1;
    }

    /// <summary>
    /// Summary information for photographer search results
    /// </summary>
    public class PhotographerSearchSummaryDto
    {
        /// <summary>
        /// Total photographers found
        /// </summary>
        public int TotalPhotographers { get; set; }

        /// <summary>
        /// Number of available photographers
        /// </summary>
        public int AvailableCount { get; set; }

        /// <summary>
        /// Number of verified photographers
        /// </summary>
        public int VerifiedCount { get; set; }

        /// <summary>
        /// Number of photographers with portfolios
        /// </summary>
        public int WithPortfolioCount { get; set; }

        /// <summary>
        /// Average rating of found photographers
        /// </summary>
        public double AverageRating { get; set; }

        /// <summary>
        /// Most common location city
        /// </summary>
        public string? MostCommonLocation { get; set; }

        /// <summary>
        /// Summary message
        /// </summary>
        public string Message => $"Found {TotalPhotographers} photographer(s): " +
                                $"{AvailableCount} available, {VerifiedCount} verified, " +
                                $"{WithPortfolioCount} with portfolios. Average rating: {AverageRating:F1}";
    }
}