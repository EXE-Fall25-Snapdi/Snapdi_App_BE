using System;
using System.ComponentModel.DataAnnotations;

namespace Snapdi.Services.DTOs
{
    /// <summary>
    /// Simplified user information for booking responses
    /// </summary>
    public class BookingUserDto
    {
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
    }

    /// <summary>
    /// Booking status information
    /// </summary>
    public class BookingStatusDto
    {
        public int StatusId { get; set; }
        public string StatusName { get; set; } = string.Empty;
    }

    /// <summary>
    /// Booking response DTO
    /// </summary>
    public class BookingResponse
    {
        public int BookingId { get; set; }
        public BookingUserDto? Customer { get; set; }
        public BookingUserDto? Photographer { get; set; }
        public DateTime ScheduleAt { get; set; }
        public string? LocationAddress { get; set; }
        public BookingStatusDto? Status { get; set; }
        public double Price { get; set; }
        public string? Note { get; set; }
    }

    /// <summary>
    /// Create booking request DTO
    /// </summary>
    public class CreateBookingRequest
    {
        [Required]
        public int CustomerId { get; set; }
        
        [Required]
        public int PhotographerId { get; set; }
        
        [Required]
        public DateTime ScheduleAt { get; set; }
        
        [MaxLength(255)]
        public string? LocationAddress { get; set; }
        
        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Price must be a positive value")]
        public double Price { get; set; }
        
        [MaxLength(1000)]
        public string? Note { get; set; }
    }
}
