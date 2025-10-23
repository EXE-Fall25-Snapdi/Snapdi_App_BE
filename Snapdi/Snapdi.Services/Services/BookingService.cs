using Microsoft.AspNetCore.SignalR;
using Snapdi.Repositories.Interfaces;
using Snapdi.Repositories.Models;
using Snapdi.Services.DTOs.RequestModels;
using Snapdi.Services.DTOs.ResponseModels;
using Snapdi.Services.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Snapdi.Services.Services
{
    public class BookingService : Interfaces.IBookingService
    {
        private readonly IBookingRepository _bookingRepo;
        private readonly IBaseRepository<User> _userRepo;
        private readonly IBaseRepository<BookingStatus> _statusRepo;
        private readonly IHubContext<BookingHub> _bookingHub;

        public BookingService(
            IBookingRepository bookingRepo,
            IBaseRepository<User> userRepo,
            IBaseRepository<BookingStatus> statusRepo,
            IHubContext<BookingHub> bookingHub)
        {
            _bookingRepo = bookingRepo;
            _userRepo = userRepo;
            _statusRepo = statusRepo;
            _bookingHub = bookingHub;
        }

        public async Task<BookingResponse> CreateBookingAsync(CreateBookingRequest request)
        {
            // Validate users exist
            var customer = await _userRepo.GetByIdAsync(request.CustomerId);
            var photographer = await _userRepo.GetByIdAsync(request.PhotographerId);

            if (customer == null || photographer == null)
                throw new ArgumentException("Customer or Photographer not found");

            // Validate status
            var status = await _statusRepo.FirstOrDefaultAsync(s => s.StatusName == "Pending");
            if (status == null)
                throw new ArgumentException("Default status 'Pending' not found");

            var booking = new Booking
            {
                CustomerId = request.CustomerId,
                PhotographerId = request.PhotographerId,
                ScheduleAt = request.ScheduleAt,
                LocationCity = request.LocationCity,
                LocationAddress = request.LocationAddress,
                Price = request.Price,
                StatusId = status.StatusId
            };

            await _bookingRepo.AddAsync(booking);
            await _bookingRepo.SaveChangesAsync();

            // Reload with navigation properties for mapping
            var created = await _bookingRepo.GetBookingWithDetailsAsync(booking.BookingId) ?? booking;
            return MapToResponse(created);
        }

        public async Task<BookingResponse> UpdateBookingStatusAsync(int bookingId, int newStatusId)
        {
            var booking = await _bookingRepo.GetByIdAsync(bookingId);
            if (booking == null)
                throw new KeyNotFoundException($"Booking ID {bookingId} not found");

            booking.StatusId = newStatusId;
            await _bookingRepo.UpdateAsync(booking);
            await _bookingRepo.SaveChangesAsync();

            var updated = await _bookingRepo.GetBookingWithDetailsAsync(bookingId) ?? booking;
            var response = MapToResponse(updated);

            // Notify the customer group about status update if we have a customerId
            if (updated.CustomerId.HasValue)
            {
                var groupName = $"customer-{updated.CustomerId.Value}";
                await _bookingHub.Clients.Group(groupName).SendAsync("bookingStatusUpdated", new
                {
                    bookingId = response.BookingId,
                    status = response.StatusName,
                    scheduleAt = response.ScheduleAt,
                    price = response.Price,
                    photographerName = response.PhotographerName
                });
            }

            return response;
        }

        public async Task<BookingResponse> GetBookingByIdAsync(int bookingId)
        {
            var booking = await _bookingRepo.GetBookingWithDetailsAsync(bookingId);
            if (booking == null)
                throw new KeyNotFoundException($"Booking ID {bookingId} not found");

            return MapToResponse(booking);
        }

        private static BookingResponse MapToResponse(Booking booking)
        {
            return new BookingResponse
            {
                BookingId = booking.BookingId,
                CustomerName = booking.Customer?.Name,
                PhotographerName = booking.Photographer?.Name,
                ScheduleAt = booking.ScheduleAt,
                StatusName = booking.Status?.StatusName,
                Price = booking.Price
            };
        }
    }
}
