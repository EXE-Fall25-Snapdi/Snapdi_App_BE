namespace Snapdi.Services.DTOs
{
    public class UserDto
    {
        public int UserId { get; set; }
        public int? RoleId { get; set; }
        public string? RoleName { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public bool IsActive { get; set; }
        public bool IsVerify { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? LocationAddress { get; set; }
        public string? LocationCity { get; set; }
        public string? AvatarUrl { get; set; }
        
        /// <summary>
        /// Current location of the user as latitude and longitude
        /// </summary>
        public LocationCoordinatesDto? CurrentLocation { get; set; }
    }

    /// <summary>
    /// Represents geographical coordinates (latitude, longitude)
    /// </summary>
    public class LocationCoordinatesDto
    {
        /// <summary>
        /// Latitude coordinate (-90 to 90)
        /// </summary>
        public double Latitude { get; set; }
        
        /// <summary>
        /// Longitude coordinate (-180 to 180)
        /// </summary>
        public double Longitude { get; set; }
    }

    public class UpdateUserStatusDto
    {
        public bool IsActive { get; set; }
        public bool IsVerify { get; set; }
    }
}