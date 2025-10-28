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
    /// Photographer info in booking responses (extends user info with profile data)
    /// </summary>
    public class BookingPhotographerDto : BookingUserDto
    {
        public double? AvgRating { get; set; }
        public bool IsAvailable { get; set; }
        public string? LevelPhotographer { get; set; }
        public double? PhotoPrice { get; set; }
        // Photographer avatar/image url from Users table
        public string? AvatarUrl { get; set; }
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
        public BookingPhotographerDto? Photographer { get; set; }
        public DateTime ScheduleAt { get; set; }
        public string? LocationAddress { get; set; }
        public BookingStatusDto? Status { get; set; }
        public double Price { get; set; }
        public string? Note { get; set; }
        public string? PhotoLink { get; set; }
    }

    /// <summary>
    /// DTO for Booking information
    /// </summary>
    public class BookingDto
    {
        public int BookingId { get; set; }
        public int? CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerEmail { get; set; }
        public string? CustomerPhone { get; set; }
        public int? PhotographerId { get; set; }
        public string? PhotographerName { get; set; }
        public string? PhotographerEmail { get; set; }
        public string? PhotographerPhone { get; set; }
        public DateTime ScheduleAt { get; set; }
        public string? LocationAddress { get; set; }
        public int? StatusId { get; set; }
        public string? StatusName { get; set; }
        public double Price { get; set; }
        public string? Note { get; set; }

        /// <summary>
        /// Photo type ID for the booking
        /// </summary>
        public int? PhotoTypeId { get; set; }

        /// <summary>
        /// Duration/time for the booking in minutes
        /// </summary>
        public int? Time { get; set; }
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

        /// <summary>
        /// Photo type ID for the booking
        /// </summary>
        [Required(ErrorMessage = "Photo type ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Photo type ID must be a positive number")]
        public int PhotoTypeId { get; set; }

        /// <summary>
        /// Duration/time for the booking in minutes
        /// </summary>
        [Required(ErrorMessage = "Duration time is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Duration time must be a positive number")]
        public int Time { get; set; }
    }

    /// <summary>
    /// DTO for creating a new booking
    /// </summary>
    public class CreateBookingDto
    {
        [Required(ErrorMessage = "Customer ID is required")]
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "Photographer ID is required")]
        public int PhotographerId { get; set; }

        [Required(ErrorMessage = "Schedule date and time is required")]
        public DateTime ScheduleAt { get; set; }

        [StringLength(255, ErrorMessage = "Location address cannot exceed 255 characters")]
        public string? LocationAddress { get; set; }

        [Required(ErrorMessage = "Price is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Price must be a positive number")]
        public double Price { get; set; }

        [StringLength(1000, ErrorMessage = "Note cannot exceed 1000 characters")]
        public string? Note { get; set; }

        /// <summary>
        /// Photo type ID for the booking
        /// </summary>
        [Required(ErrorMessage = "Photo type ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Photo type ID must be a positive number")]
        public int PhotoTypeId { get; set; }

        /// <summary>
        /// Duration/time for the booking in minutes
        /// </summary>
        [Required(ErrorMessage = "Duration time is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Duration time must be a positive number")]
        public int Time { get; set; }
    }

    /// <summary>
    /// DTO for updating a booking
    /// </summary>
    public class UpdateBookingDto
    {
        public DateTime? ScheduleAt { get; set; }

        [StringLength(255, ErrorMessage = "Location address cannot exceed 255 characters")]
        public string? LocationAddress { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Price must be a positive number")]
        public double? Price { get; set; }

        [StringLength(1000, ErrorMessage = "Note cannot exceed 1000 characters")]
        public string? Note { get; set; }
    }

    /// <summary>
    /// DTO for updating booking status
    /// </summary>
    public class UpdateBookingStatusDto
    {
        [Required(ErrorMessage = "Status ID is required")]
        public int StatusId { get; set; }
    }

    /// <summary>
    /// DTO for searching bookings with filtering and sorting
    /// </summary>
    public class BookingSearchDto
    {
        /// <summary>
        /// Search term for customer name, photographer name, or location
        /// </summary>
        public string? SearchTerm { get; set; }

        /// <summary>
        /// Filter by customer ID
        /// </summary>
        public int? CustomerId { get; set; }

        /// <summary>
        /// Filter by photographer ID
        /// </summary>
        public int? PhotographerId { get; set; }

        /// <summary>
        /// Filter by status ID
        /// </summary>
        public int? StatusId { get; set; }

        /// <summary>
        /// Filter by location address
        /// </summary>
        public string? LocationAddress { get; set; }

        /// <summary>
        /// Filter by minimum price
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "Minimum price must be a positive number")]
        public double? MinPrice { get; set; }

        /// <summary>
        /// Filter by maximum price
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "Maximum price must be a positive number")]
        public double? MaxPrice { get; set; }

        /// <summary>
        /// Filter by schedule date from
        /// </summary>
        public DateTime? ScheduleFrom { get; set; }

        /// <summary>
        /// Filter by schedule date to
        /// </summary>
        public DateTime? ScheduleTo { get; set; }

        /// <summary>
        /// Sort by field: "scheduleAt", "price", "customerName", "photographerName", "statusName"
        /// </summary>
        [RegularExpression(@"^(scheduleAt|price|customerName|photographerName|statusName)$",
            ErrorMessage = "Sort by must be 'scheduleAt', 'price', 'customerName', 'photographerName', or 'statusName'")]
        public string? SortBy { get; set; } = "scheduleAt";

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
    /// Response DTO for booking search results
    /// </summary>
    public class BookingSearchResultDto
    {
        /// <summary>
        /// List of bookings matching the search criteria
        /// </summary>
        public List<BookingDto> Data { get; set; } = new();

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
    /// DTO for booking status notification via SignalR
    /// </summary>
    public class BookingStatusNotificationDto
    {
        public int BookingId { get; set; }
        public int? OldStatusId { get; set; }
        public string? OldStatusName { get; set; }
        public int NewStatusId { get; set; }
        public string NewStatusName { get; set; } = string.Empty;
        public DateTime ChangedAt { get; set; }
        public string? CustomerName { get; set; }
        public string? PhotographerName { get; set; }
        public DateTime ScheduleAt { get; set; }
        public double Price { get; set; }
    }

    public class PhotoLinkUpdateDto
    {
        public string PhotoLink { get; set; } = string.Empty;
    }

    public class PhotoLinkUpdateNotificationDto
    {
        public int BookingId { get; set; }
        public int PhotographerId { get; set; }
        public int? CustomerId { get; set; }
        public string PhotoLink { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// DTO for booking status statistics with status name
    /// </summary>
    public class BookingStatusStatisticsDto
    {
        public int StatusId { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public int Count { get; set; }
        public double Percentage { get; set; }
    }

    /// <summary>
    /// Response DTO for booking statistics
    /// </summary>
    public class BookingStatisticsResponseDto
    {
        /// <summary>
        /// Total number of bookings
        /// </summary>
        public int TotalBookings { get; set; }

        /// <summary>
        /// Statistics by status
        /// </summary>
        public List<BookingStatusStatisticsDto> StatusStatistics { get; set; } = new();

        /// <summary>
        /// When the statistics were generated
        /// </summary>
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    }
}
