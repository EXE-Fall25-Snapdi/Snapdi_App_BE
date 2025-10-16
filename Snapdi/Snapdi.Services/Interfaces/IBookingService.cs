using Snapdi.Services.DTOs.RequestModels;
using Snapdi.Services.DTOs.ResponseModels;
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
    }
}
