using AutoMapper;
using Snapdi.Repositories.Interfaces;
using Snapdi.Repositories.Models;
using Snapdi.Services.DTOs.RequestModels;
using Snapdi.Services.DTOs.ResponseModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snapdi.Services.Services
{
    public  class BookingService : Interfaces.IBookingService
    {
        private readonly IBookingRepository _bookingRepo;
        private readonly IBaseRepository<User> _userRepo;
        private readonly IBaseRepository<BookingStatus> _statusRepo;
        private readonly IMapper _mapper;

        public BookingService(
            IBookingRepository bookingRepo,
            IBaseRepository<User> userRepo,
            IBaseRepository<BookingStatus> statusRepo,
            IMapper mapper)
        {
            _bookingRepo = bookingRepo;
            _userRepo = userRepo;
            _statusRepo = statusRepo;
            _mapper = mapper;
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

            return _mapper.Map<BookingResponse>(booking);
        }

        public async Task<BookingResponse> UpdateBookingStatusAsync(int bookingId, int newStatusId)
        {
            var booking = await _bookingRepo.GetByIdAsync(bookingId);
            if (booking == null)
                throw new KeyNotFoundException($"Booking ID {bookingId} not found");

            booking.StatusId = newStatusId;
            await _bookingRepo.UpdateAsync(booking);
            await _bookingRepo.SaveChangesAsync();

            return _mapper.Map<BookingResponse>(booking);
        }

        public async Task<BookingResponse> GetBookingByIdAsync(int bookingId)
        {
            var booking = await _bookingRepo.GetByIdAsync(bookingId);
            if (booking == null)
                throw new KeyNotFoundException($"Booking ID {bookingId} not found");
            return _mapper.Map<BookingResponse>(booking);
        }
    }
}
