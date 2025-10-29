using System;
using System.Collections.Generic;

namespace Snapdi.Services.DTOs
{
    /// <summary>
    /// Simple user information for pending booking
    /// </summary>
    public class PendingBookingUserDto
    {
        public int UserId { get; set; }
      public string? AvatarUrl { get; set; }
        
        /// <summary>
        /// User's full name
      /// </summary>
        public string? Name { get; set; }
        
        /// <summary>
 /// User's email address
/// </summary>
   public string? Email { get; set; }
        
 /// <summary>
    /// User's phone number
  /// </summary>
    public string? Phone { get; set; }
    }

    /// <summary>
    /// Booking status information
    /// </summary>
    public class PendingBookingStatusDto
    {
        public int StatusId { get; set; }
        public string StatusName { get; set; } = string.Empty;
    }

    /// <summary>
    /// Photo type information for pending booking
  /// </summary>
    public class PendingBookingPhotoTypeDto
    {
 public int PhotoTypeId { get; set; }
 public string PhotoTypeName { get; set; } = string.Empty;
    public double? PhotoPrice { get; set; }
     public int? Time { get; set; }
    }

    /// <summary>
    /// Photographer information for pending booking (extends user info)
    /// </summary>
    public class PendingBookingPhotographerDto : PendingBookingUserDto
    {
        /// <summary>
  /// Photographer's average rating
        /// </summary>
        public double? AvgRating { get; set; }
        
        /// <summary>
        /// Photographer's availability status
        /// </summary>
   public bool IsAvailable { get; set; }
        
        /// <summary>
 /// Photographer's level (e.g., Beginner, Professional)
        /// </summary>
    public string? LevelPhotographer { get; set; }
    }

    /// <summary>
  /// Pending booking response for photographer
    /// </summary>
    public class PendingBookingResponseDto
    {
 /// <summary>
        /// Booking ID
        /// </summary>
   public int BookingId { get; set; }

        /// <summary>
        /// Customer information (basic with contact details)
        /// </summary>
  public PendingBookingUserDto? User { get; set; }

        /// <summary>
    /// Photographer information (extended with profile data)
        /// </summary>
     public PendingBookingPhotographerDto? Photographer { get; set; }

        /// <summary>
  /// Scheduled date and time for the booking
   /// </summary>
        public DateTime ScheduleAt { get; set; }

 /// <summary>
 /// Location address for the booking
 /// </summary>
 public string? LocationAddress { get; set; }

        /// <summary>
  /// Total price for the booking
 /// </summary>
     public double Price { get; set; }

  /// <summary>
        /// Current booking status
    /// </summary>
  public PendingBookingStatusDto? Status { get; set; }

      /// <summary>
 /// Duration of the booking in minutes
   /// </summary>
   public int? Duration { get; set; }

        /// <summary>
        /// Photo type requested
        /// </summary>
        public PendingBookingPhotoTypeDto? PhotoType { get; set; }

        /// <summary>
      /// Additional notes
        /// </summary>
    public string? Note { get; set; }

        /// <summary>
  /// Photo link/URL for the completed booking
        /// </summary>
      public string? PhotoLink { get; set; }
    }

    /// <summary>
    /// Paginated response for photographer pending bookings
    /// </summary>
    public class PhotographerPendingBookingsResponseDto
    {
        /// <summary>
     /// List of pending bookings
        /// </summary>
        public List<PendingBookingResponseDto> Data { get; set; } = new();

        /// <summary>
        /// Total number of pending bookings
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
    public int TotalPages { get; set; }

        /// <summary>
  /// Whether there is a next page
        /// </summary>
        public bool HasNextPage => CurrentPage < TotalPages;

   /// <summary>
     /// Whether there is a previous page
        /// </summary>
        public bool HasPreviousPage => CurrentPage > 1;
    }
}
