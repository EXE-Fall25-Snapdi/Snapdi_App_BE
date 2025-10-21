namespace Snapdi.Services.DTOs
{
    public class UserWithPhotographerDto : UserDto
    {
        public PhotographerProfileDto? PhotographerProfile { get; set; }
        public List<PhotoPortfolioDto>? PhotoPortfolios { get; set; }
    }

    public class PhotographerProfileDto
    {
        public int UserId { get; set; }
        public string? EquipmentDescription { get; set; }
        public string? YearsOfExperience { get; set; }
        public double? AvgRating { get; set; }
        public bool IsAvailable { get; set; }
        public string? Description { get; set; }
        public string? LevelPhotographer { get; set; }
        public double? PhotoPrice { get; set; }
        public List<PhotoTypeDto>? PhotoTypes { get; set; }
        public string? WorkLocation { get; set; }
        /// <summary>
        /// List of photography styles associated with this photographer
        /// </summary>
        public List<StyleDto>? PhotographerStyles { get; set; }
    }

    public class PhotoPortfolioDto
    {
        public int PhotoPortfolioId { get; set; }
        public int UserId { get; set; }
        public string PhotoUrl { get; set; } = string.Empty;
    }

    public class CreatePhotographerProfileDto
    {
        public string? EquipmentDescription { get; set; }
        public string? YearsOfExperience { get; set; }
        public bool IsAvailable { get; set; } = true;
        public string? Description { get; set; }
        public string? LevelPhotographer { get; set; } = string.Empty;
        public double? PhotoPrice { get; set; }
        public List<int>? PhotoTypeIds { get; set; }
        public string? WorkLocation { get; set; }
    }

    public class UpdatePhotographerProfileDto
    {
        public string? EquipmentDescription { get; set; }
        public string? YearsOfExperience { get; set; }
        public double? AvgRating { get; set; }
        public bool? IsAvailable { get; set; }
        public string? Description { get; set; }
        public double? PhotoPrice { get; set; }
        public List<int>? PhotoTypeIds { get; set; }
        public string? WorkLocation { get; set; }
    }

    public class CreatePhotoPortfolioDto
    {
        public string PhotoUrl { get; set; } = string.Empty;
    }

    public class UpdatePhotoPortfolioDto
    {
        public string? PhotoUrl { get; set; }
    }

    /// <summary>
    /// DTO for creating multiple photo portfolios at once
    /// </summary>
    public class CreateMultiplePhotoPortfolioDto
    {
        /// <summary>
        /// List of photo URLs to upload
        /// </summary>
        public List<string> PhotoUrls { get; set; } = new List<string>();
    }

    /// <summary>
    /// Response DTO for multiple photo portfolio creation
    /// </summary>
    public class CreateMultiplePhotoPortfolioResponseDto
    {
        /// <summary>
        /// List of successfully created photo portfolios
        /// </summary>
        public List<PhotoPortfolioDto> CreatedPortfolios { get; set; } = new List<PhotoPortfolioDto>();

        /// <summary>
        /// List of photo URLs that failed to create
        /// </summary>
        public List<string> FailedPhotoUrls { get; set; } = new List<string>();

        /// <summary>
        /// Total number of photos attempted
        /// </summary>
        public int TotalAttempted { get; set; }

        /// <summary>
        /// Number of successfully created portfolios
        /// </summary>
        public int SuccessCount { get; set; }

        /// <summary>
        /// Number of failed creations
        /// </summary>
        public int FailedCount { get; set; }

        /// <summary>
        /// Overall success status
        /// </summary>
        public bool IsCompleteSuccess => FailedCount == 0;

        /// <summary>
        /// Success message
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }
}