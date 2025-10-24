using System.ComponentModel.DataAnnotations;

namespace Snapdi.Services.DTOs
{
    /// <summary>
    /// DTO for finding snappers (photographers) with simple filters
    /// </summary>
    public class FindSnapperDto
    {
        /// <summary>
        /// Filter by work location (optional)
        /// Searches in photographer's preferred work location/service area
        /// </summary>
        [MaxLength(255, ErrorMessage = "Work location cannot exceed 255 characters")]
        public string? WorkLocation { get; set; }

        /// <summary>
        /// Latitude of the search center point (optional)
        /// Required when using radius-based search
        /// Example: 10.8231 (Ho Chi Minh City)
        /// </summary>
        [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90")]
        public double? Latitude { get; set; }

        /// <summary>
        /// Longitude of the search center point (optional)
        /// Required when using radius-based search
        /// Example: 106.6297 (Ho Chi Minh City)
        /// </summary>
        [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180")]
        public double? Longitude { get; set; }

        /// <summary>
        /// Search radius in kilometers (optional, default: 5)
        /// Filters photographers within this distance from the specified location
        /// Only used when Latitude and Longitude are provided
        /// </summary>
        [Range(0.1, 100, ErrorMessage = "Radius must be between 0.1 and 100 kilometers")]
        public double? RadiusInKm { get; set; } = 5;

        /// <summary>
        /// Minimum price range for photo services (optional)
        /// Filters photographers with PhotoPrice >= MinPrice
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "Minimum price must be a positive number")]
        public double? MinPrice { get; set; }

        /// <summary>
        /// Maximum price range for photo services (optional)
        /// Filters photographers with PhotoPrice <= MaxPrice
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "Maximum price must be a positive number")]
        public double? MaxPrice { get; set; }

        /// <summary>
        /// Filter by photo type IDs (optional)
        /// Example: [1, 2, 3] for specific photo types
        /// </summary>
        public List<int>? PhotoTypeIds { get; set; }

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
        /// Sort by field: "name", "rating", "worklocation", "price", "distance"
        /// Note: "distance" only works when using radius-based search
        /// </summary>
        [RegularExpression(@"^(name|rating|worklocation|price|distance)$", 
            ErrorMessage = "Sort by must be 'name', 'rating', 'worklocation', 'price', or 'distance'")]
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
        public string? WorkLocation { get; set; }

        // Location Information
        public LocationCoordinatesDto? CurrentLocation { get; set; }
        public double? DistanceInKm { get; set; } // Distance from search point (only populated in nearby search)

        // Photo Types Information
        public List<PhotoTypeDto>? PhotoTypes { get; set; }

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

    /// <summary>
    /// DTO for photo type information
    /// </summary>
    public class PhotoTypeDto
    {
        public int PhotoTypeId { get; set; }
        public string PhotoTypeName { get; set; } = null!;
    }
}
