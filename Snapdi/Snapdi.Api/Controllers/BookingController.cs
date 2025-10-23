using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Snapdi.Services.DTOs.RequestModels;
using Snapdi.Services.Interfaces;

namespace Snapdi.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public BookingController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateBooking([FromBody] CreateBookingRequest request)
        {
            var result = await _bookingService.CreateBookingAsync(request);
            return CreatedAtAction(nameof(GetBookingById), new { id = result.BookingId }, result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBookingById(int id)
        {
            var result = await _bookingService.GetBookingByIdAsync(id);
            return Ok(result);
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateBookingStatus(int id, [FromBody] int statusId)
        {
            var result = await _bookingService.UpdateBookingStatusAsync(id, statusId);
            return Ok(result);
        }
    }
}
