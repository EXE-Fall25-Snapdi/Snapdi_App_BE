using System.ComponentModel.DataAnnotations;

namespace Snapdi.Services.DTOs
{
    /// <summary>
    /// DTO for photographer registration - includes both user info and photographer-specific fields
    /// </summary>
    public class CreatePhotographerDto
    {
        // User basic information (same as CreateUserDto but with required fields)
        [Required(ErrorMessage = "Name is required")]
        [MaxLength(255, ErrorMessage = "Name cannot exceed 255 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Please provide a valid email address")]
        [MaxLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Please provide a valid phone number")]
        [MaxLength(50, ErrorMessage = "Phone cannot exceed 50 characters")]
        public string? Phone { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        [MaxLength(255, ErrorMessage = "Password cannot exceed 255 characters")]
        public string Password { get; set; } = string.Empty;

        // Location information - city is required, address is optional
        [MaxLength(255, ErrorMessage = "Location address cannot exceed 255 characters")]
        public string? LocationAddress { get; set; }

        [Required(ErrorMessage = "Location city is required for photographers")]
        [MaxLength(100, ErrorMessage = "Location city cannot exceed 100 characters")]
        public string LocationCity { get; set; } = string.Empty;

        [MaxLength(255, ErrorMessage = "Avatar URL cannot exceed 255 characters")]
        public string? AvatarUrl { get; set; }
        
        /// <summary>
        /// Initial location coordinates for the photographer
        /// </summary>
        public LocationCoordinatesDto? CurrentLocation { get; set; }

        // Photographer-specific required fields
        [Required(ErrorMessage = "Years of experience is required for photographers")]
        [MaxLength(100, ErrorMessage = "Years of experience cannot exceed 100 characters")]
        public string YearsOfExperience { get; set; } = string.Empty;

        [Required(ErrorMessage = "Equipment description is required for photographers")]
        [MaxLength(500, ErrorMessage = "Equipment description cannot exceed 500 characters")]
        public string EquipmentDescription { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        public bool IsAvailable { get; set; } = false;

        // New fields for photographer
        [Range(0, double.MaxValue, ErrorMessage = "Photo price must be a positive number")]
        public double? PhotoPrice { get; set; }

        public List<int>? PhotoTypeIds { get; set; }

        [MaxLength(255, ErrorMessage = "Work location cannot exceed 255 characters")]
        public string? WorkLocation { get; set; }
    }

    /// <summary>
    /// Response DTO for photographer registration
    /// </summary>
    public class PhotographerRegistrationResponseDto
    {
        public UserWithPhotographerDto User { get; set; } = null!;
        public string Message { get; set; } = string.Empty;
    }
}