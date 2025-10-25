using Snapdi.Services.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snapdi.Services.Interfaces
{
    public interface IBookingService
    {
        Task<BookingResponse> CreateBookingAsync(CreateBookingRequest request);
        Task<BookingResponse> UpdateBookingStatusAsync(int bookingId, int newStatusId);
        Task<BookingResponse> GetBookingByIdAsync(int bookingId);

        /// <summary>
        /// Get all bookings
        /// </summary>
        Task<IEnumerable<BookingDto>> GetAllBookingsAsync();

        /// <summary>
        /// Get bookings by customer ID
        /// </summary>
        Task<IEnumerable<BookingDto>> GetBookingsByCustomerAsync(int customerId);

        /// <summary>
        /// Get bookings by photographer ID
        /// </summary>
        Task<IEnumerable<BookingDto>> GetBookingsByPhotographerAsync(int photographerId);

        /// <summary>
        /// Get bookings by status ID
        /// </summary>
        Task<IEnumerable<BookingDto>> GetBookingsByStatusAsync(int statusId);

        /// <summary>
        /// Search bookings with filtering, sorting, and pagination
        /// </summary>
        Task<BookingSearchResultDto> SearchBookingsAsync(BookingSearchDto searchDto);

        /// <summary>
        /// Create a new booking (alternative method)
        /// </summary>
        Task<BookingDto> CreateBookingDtoAsync(CreateBookingDto createDto);

        /// <summary>
        /// Update a booking
        /// </summary>
        Task<BookingDto?> UpdateBookingAsync(int bookingId, UpdateBookingDto updateDto);

        /// <summary>
        /// Update booking status (alternative method)
        /// </summary>
        Task<BookingDto?> UpdateBookingStatusDtoAsync(int bookingId, UpdateBookingStatusDto statusDto);

        /// <summary>
        /// Delete a booking
        /// </summary>
        Task<bool> DeleteBookingAsync(int bookingId);

        /// <summary>
        /// Check if booking exists
        /// </summary>
        Task<bool> BookingExistsAsync(int bookingId);

        /// <summary>
        /// Get bookings count by status (simple version)
        /// </summary>
        Task<Dictionary<int, int>> GetBookingsCountByStatusAsync();

        /// <summary>
        /// Get detailed booking statistics with status names and percentages
        /// </summary>
        Task<BookingStatisticsResponseDto> GetBookingStatisticsAsync();

        /// <summary>
        /// Get bookings by current user id with pagination
        /// </summary>
        Task<PagedResultDto<BookingResponse>> GetMyBookingsAsync(int currentUserId, int page, int pageSize);
    }
}
