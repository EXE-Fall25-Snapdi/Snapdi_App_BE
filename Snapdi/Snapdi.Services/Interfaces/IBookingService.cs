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
        // New: get bookings by current user id
        Task<IEnumerable<BookingResponse>> GetMyBookingsAsync(int currentUserId);
    }
}
