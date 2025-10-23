using System.ComponentModel.DataAnnotations;

namespace Snapdi.Services.DTOs
{
    /// <summary>
    /// DTO for finding snappers nearby for map display
    /// </summary>
    public class FindSnappersNearbyDto
    {
        /// <summary>
        /// Latitude of the search center point (required)
        /// Example: 10.8231 (Ho Chi Minh City)
        /// </summary>
        [Required(ErrorMessage = "Latitude is required for nearby search")]
        [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90")]
        public double Latitude { get; set; }

        /// <summary>
        /// Longitude of the search center point (required)
        /// Example: 106.6297 (Ho Chi Minh City)
        /// </summary>
        [Required(ErrorMessage = "Longitude is required for nearby search")]
        [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180")]
        public double Longitude { get; set; }

        /// <summary>
        /// Search radius in kilometers (default: 5)
        /// Filters photographers within this distance from the specified location
        /// </summary>
        [Range(0.1, 100, ErrorMessage = "Radius must be between 0.1 and 100 kilometers")]
        public double RadiusInKm { get; set; } = 5;

        /// <summary>
        /// Minimum price range for photo services (optional)
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "Minimum price must be a positive number")]
        public double? MinPrice { get; set; }

        /// <summary>
        /// Maximum price range for photo services (optional)
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "Maximum price must be a positive number")]
        public double? MaxPrice { get; set; }

        /// <summary>
        /// Filter by photo type IDs (optional)
        /// </summary>
        public List<int>? PhotoTypeIds { get; set; }

        /// <summary>
        /// Filter by photography style IDs (optional)
        /// </summary>
        public List<int>? StyleIds { get; set; }

        /// <summary>
        /// Filter by availability status (optional)
        /// Default: true (only show available photographers)
        /// </summary>
        public bool? IsAvailable { get; set; } = true;

        /// <summary>
        /// Maximum number of results to return (default: 50, max: 200)
        /// </summary>
        [Range(1, 200, ErrorMessage = "Limit must be between 1 and 200")]
        public int Limit { get; set; } = 50;
    }

    /// <summary>
    /// Simplified DTO for displaying photographer on map
    /// </summary>
    public class SnapperMapDto
    {
        /// <summary>
        /// User ID of the photographer
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Photographer name
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// Avatar URL for map marker
        /// </summary>
        public string? AvatarUrl { get; set; }

        /// <summary>
        /// Current location coordinates
        /// </summary>
        public LocationCoordinatesDto CurrentLocation { get; set; } = null!;

        /// <summary>
        /// Distance from search center in kilometers
        /// </summary>
        public double DistanceInKm { get; set; }

        /// <summary>
        /// Photographer level
        /// </summary>
        public string? LevelPhotographer { get; set; }

        /// <summary>
        /// Average rating (0-5)
        /// </summary>
        public double? AvgRating { get; set; }

        /// <summary>
        /// Photo service price
        /// </summary>
        public double? PhotoPrice { get; set; }

        /// <summary>
        /// Availability status
        /// </summary>
        public bool IsAvailable { get; set; }

        /// <summary>
        /// Brief description (truncated for map display)
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Location city
        /// </summary>
        public string? LocationCity { get; set; }

        /// <summary>
        /// Photo types offered
        /// </summary>
        public List<string>? PhotoTypes { get; set; }

        /// <summary>
        /// Photography styles
        /// </summary>
        public List<string>? Styles { get; set; }
    }

    /// <summary>
    /// Response DTO for nearby snappers map display
    /// </summary>
    public class FindSnappersNearbyResultDto
    {
        /// <summary>
        /// List of snappers within the specified radius
        /// </summary>
        public List<SnapperMapDto> Snappers { get; set; } = new();

        /// <summary>
        /// Total number of snappers found within radius
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Search center coordinates
        /// </summary>
        public LocationCoordinatesDto SearchCenter { get; set; } = null!;

        /// <summary>
        /// Search radius in kilometers
        /// </summary>
        public double RadiusInKm { get; set; }

        /// <summary>
        /// Number of available snappers in results
        /// </summary>
        public int AvailableCount { get; set; }

        /// <summary>
        /// Summary message
        /// </summary>
        public string Summary => $"Found {TotalCount} snapper(s) within {RadiusInKm} km, {AvailableCount} available";
    }
}
