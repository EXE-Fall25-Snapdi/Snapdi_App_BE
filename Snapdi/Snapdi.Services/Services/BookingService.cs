using Microsoft.AspNetCore.SignalR;
using Snapdi.Repositories.Interfaces;
using Snapdi.Repositories.Models;
using Snapdi.Services.Hubs;
using Snapdi.Services.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Snapdi.Services.Services
{
    public class BookingService : Interfaces.IBookingService
    {
        private readonly IBookingRepository _bookingRepo;
        private readonly IUserRepository _userRepo;
        private readonly IBookingStatusRepository _statusRepo;
        private readonly IHubContext<BookingHub> _bookingHub;

        public BookingService(
            IBookingRepository bookingRepo,
            IUserRepository userRepo,
            IBookingStatusRepository statusRepo,
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
            var status = await _statusRepo.GetByNameAsync("Pending");
            if (status == null)
                throw new ArgumentException("Default status 'Pending' not found");

            var booking = new Booking
            {
                CustomerId = request.CustomerId,
                PhotographerId = request.PhotographerId,
                ScheduleAt = request.ScheduleAt,
                LocationAddress = request.LocationAddress,
                Price = request.Price,
                StatusId = status.StatusId,
                Note = request.Note
            };

            await _bookingRepo.AddAsync(booking);
            await _bookingRepo.SaveChangesAsync();

            // Reload booking with related entities for mapping
            var createdBooking = await _bookingRepo.GetBookingWithDetailsAsync(booking.BookingId);
            
            return MapToBookingResponse(createdBooking!);
        }

        public async Task<BookingResponse> UpdateBookingStatusAsync(int bookingId, int newStatusId)
        {
            var booking = await _bookingRepo.GetBookingWithDetailsAsync(bookingId);
            if (booking == null)
                throw new KeyNotFoundException($"Booking ID {bookingId} not found");

            booking.StatusId = newStatusId;
            await _bookingRepo.UpdateAsync(booking);
            await _bookingRepo.SaveChangesAsync();

            // Reload to get updated status name
            var updatedBooking = await _bookingRepo.GetBookingWithDetailsAsync(bookingId);
            var response = MapToBookingResponse(updatedBooking!);

            // Notify the customer group about status update if we have a customerId
            if (updatedBooking.CustomerId.HasValue)
            {
                var groupName = $"customer-{updatedBooking.CustomerId.Value}";
                await _bookingHub.Clients.Group(groupName).SendAsync("bookingStatusUpdated", new
                {
                    bookingId = response.BookingId,
                    statusId = response.Status?.StatusId,
                    statusName = response.Status?.StatusName,
                    scheduleAt = response.ScheduleAt,
                    price = response.Price,
                    photographerId = response.Photographer?.UserId,
                    photographerName = response.Photographer?.Name,
                    customerName = response.Customer?.Name,
                    locationAddress = response.LocationAddress,
                    note = response.Note
                });
            }

            return response;
        }

        public async Task<BookingResponse> GetBookingByIdAsync(int bookingId)
        {
            var booking = await _bookingRepo.GetBookingWithDetailsAsync(bookingId);
            if (booking == null)
                throw new KeyNotFoundException($"Booking ID {bookingId} not found");
            return MapToBookingResponse(booking);
        }

        #region Private Methods

        private static BookingResponse MapToBookingResponse(Booking booking)
        {
            return new BookingResponse
            {
                BookingId = booking.BookingId,
                Customer = booking.Customer != null ? MapToBookingUserDto(booking.Customer) : null,
                Photographer = booking.Photographer != null ? MapToBookingUserDto(booking.Photographer) : null,
                ScheduleAt = booking.ScheduleAt,
                LocationAddress = booking.LocationAddress,
                Status = booking.Status != null ? MapToBookingStatusDto(booking.Status) : null,
                Price = booking.Price,
                Note = booking.Note
            };
        }

        private static BookingUserDto MapToBookingUserDto(User user)
        {
            return new BookingUserDto
            {
                UserId = user.UserId,
                Name = user.Name,
                Email = user.Email,
                Phone = string.IsNullOrEmpty(user.Phone) ? null : user.Phone
            };
        }

        private static BookingStatusDto MapToBookingStatusDto(BookingStatus status)
        {
            return new BookingStatusDto
            {
                StatusId = status.StatusId,
                StatusName = status.StatusName
            };
        }

        #endregion
    }
}
