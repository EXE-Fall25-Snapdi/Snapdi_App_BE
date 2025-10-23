using System.ComponentModel.DataAnnotations;

namespace Snapdi.Services.DTOs
{
    /// <summary>
    /// DTO for updating photographer availability status and location
    /// </summary>
    public class UpdatePhotographerStatusDto
    {
        /// <summary>
        /// Photographer availability status
        /// </summary>
        /// <example>true</example>
        [Required(ErrorMessage = "IsAvailable is required")]
        public bool IsAvailable { get; set; }

        /// <summary>
        /// Current location coordinates from GPS (optional)
        /// Updates the photographer's real-time location
        /// </summary>
        public LocationCoordinatesDto? CurrentLocation { get; set; }
    }
}
