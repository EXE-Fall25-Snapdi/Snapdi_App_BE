using Snapdi.Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snapdi.Repositories.Interfaces
{
    public interface IBookingRepository : IBaseRepository<Models.Booking>
    {
        /// <summary>
        /// Get booking with related entities (Customer, Photographer, Status)
        /// </summary>
        Task<Booking?> GetBookingWithDetailsAsync(int bookingId);

        /// <summary>
        /// Get bookings by customer ID
        /// </summary>
        Task<IEnumerable<Booking>> GetBookingsByCustomerAsync(int customerId);

        /// <summary>
        /// Get bookings by photographer ID
        /// </summary>
        Task<IEnumerable<Booking>> GetBookingsByPhotographerAsync(int photographerId);

        /// <summary>
        /// Get bookings by status ID
        /// </summary>
        Task<IEnumerable<Booking>> GetBookingsByStatusAsync(int statusId);

        /// <summary>
        /// Search bookings with filtering, sorting, and pagination
        /// </summary>
        Task<(IEnumerable<Booking> Bookings, int TotalCount)> SearchBookingsAsync(
            int page,
            int pageSize,
            string? searchTerm = null,
            int? customerId = null,
            int? photographerId = null,
            int? statusId = null,
            string? locationAddress = null,
            double? minPrice = null,
            double? maxPrice = null,
            DateTime? scheduleFrom = null,
            DateTime? scheduleTo = null,
            string? sortBy = "scheduleAt",
            string? sortDirection = "desc");

        /// <summary>
        /// Update booking status
        /// </summary>
        Task UpdateBookingStatusAsync(int bookingId, int statusId);

        /// <summary>
        /// Check if booking exists
        /// </summary>
        Task<bool> BookingExistsAsync(int bookingId);

        /// <summary>
        /// Get bookings count by status
        /// </summary>
        Task<Dictionary<int, int>> GetBookingsCountByStatusAsync();

        /// <summary>
        /// Get bookings count by status with status names
        /// </summary>
        Task<List<(int StatusId, string StatusName, int Count)>> GetBookingsCountByStatusWithNamesAsync();
    }
}
