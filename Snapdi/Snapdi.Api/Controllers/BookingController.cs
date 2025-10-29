using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Snapdi.Api.Hubs;
using Snapdi.Services.DTOs;
using Snapdi.Services.Interfaces;
using System.Security.Claims;

namespace Snapdi.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;
        private readonly IHubContext<BookingHub> _bookingHubContext;

        public BookingController(
            IBookingService bookingService,
            IHubContext<BookingHub> bookingHubContext)
        {
            _bookingService = bookingService;
            _bookingHubContext = bookingHubContext;
        }

        /// <summary>
        /// Get all bookings (Admin only)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<IEnumerable<BookingDto>>> GetAllBookings()
        {
            try
            {
                var bookings = await _bookingService.GetAllBookingsAsync();
                return Ok(bookings);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "Internal server error",
                    message = "An error occurred while retrieving bookings",
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Get booking by ID
        /// </summary>
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<BookingDto>> GetBookingById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { error = "Invalid booking ID", message = "Booking ID must be a positive number" });
                }

                // Use GetAllBookingsAsync and filter by ID, or add a new method
                var bookings = await _bookingService.GetAllBookingsAsync();
                var booking = bookings.FirstOrDefault(b => b.BookingId == id);

                if (booking == null)
                {
                    return NotFound(new { error = "Booking not found", message = $"Booking with ID {id} does not exist" });
                }

                // Check authorization - user can only view their own bookings or admin can view all
                var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

                if (currentUserIdClaim != null && int.TryParse(currentUserIdClaim.Value, out int currentUserId))
                {
                    if (currentUserRole != "ADMIN" &&
                        booking.CustomerId != currentUserId &&
                        booking.PhotographerId != currentUserId)
                    {
                        return Forbid();
                    }
                }

                return Ok(booking);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "Internal server error",
                    message = "An error occurred while retrieving the booking",
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Get bookings by customer ID
        /// </summary>
        [HttpGet("customer/{customerId}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<BookingDto>>> GetBookingsByCustomer(int customerId)
        {
            try
            {
                if (customerId <= 0)
                {
                    return BadRequest(new { error = "Invalid customer ID", message = "Customer ID must be a positive number" });
                }

                // Check authorization
                var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

                if (currentUserIdClaim != null && int.TryParse(currentUserIdClaim.Value, out int currentUserId))
                {
                    if (currentUserRole != "ADMIN" && currentUserId != customerId)
                    {
                        return Forbid();
                    }
                }

                var bookings = await _bookingService.GetBookingsByCustomerAsync(customerId);
                return Ok(bookings);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "Internal server error",
                    message = "An error occurred while retrieving customer bookings",
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Get bookings by photographer ID
        /// </summary>
        [HttpGet("photographer/{photographerId}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<BookingDto>>> GetBookingsByPhotographer(int photographerId)
        {
            try
            {
                if (photographerId <= 0)
                {
                    return BadRequest(new { error = "Invalid photographer ID", message = "Photographer ID must be a positive number" });
                }

                // Check authorization
                var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

                if (currentUserIdClaim != null && int.TryParse(currentUserIdClaim.Value, out int currentUserId))
                {
                    if (currentUserRole != "ADMIN" && currentUserId != photographerId)
                    {
                        return Forbid();
                    }
                }

                var bookings = await _bookingService.GetBookingsByPhotographerAsync(photographerId);
                return Ok(bookings);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "Internal server error",
                    message = "An error occurred while retrieving photographer bookings",
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Get bookings by status ID (Admin only)
        /// </summary>
        [HttpGet("status/{statusId}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<IEnumerable<BookingDto>>> GetBookingsByStatus(int statusId)
        {
            try
            {
                if (statusId <= 0)
                {
                    return BadRequest(new { error = "Invalid status ID", message = "Status ID must be a positive number" });
                }

                var bookings = await _bookingService.GetBookingsByStatusAsync(statusId);
                return Ok(bookings);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "Internal server error",
                    message = "An error occurred while retrieving bookings by status",
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Search bookings with filtering and sorting (Admin only)
        /// </summary>
        [HttpPost("search")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<BookingSearchResultDto>> SearchBookings([FromBody] BookingSearchDto searchDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        error = "Validation failed",
                        message = "Please check your input data",
                        details = ModelState.Where(x => x.Value!.Errors.Count > 0)
                            .ToDictionary(
                                kvp => kvp.Key,
                                kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                            )
                    });
                }

                var result = await _bookingService.SearchBookingsAsync(searchDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "Internal server error",
                    message = "An error occurred while searching bookings",
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Create a new booking
        /// </summary>
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<BookingDto>> CreateBooking([FromBody] CreateBookingDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        error = "Validation failed",
                        message = "Please check your input data",
                        details = ModelState.Where(x => x.Value!.Errors.Count > 0)
                            .ToDictionary(
                                kvp => kvp.Key,
                                kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                            )
                    });
                }

                // Check authorization - user can only create bookings for themselves
                var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

                if (currentUserIdClaim != null && int.TryParse(currentUserIdClaim.Value, out int currentUserId))
                {
                    if (currentUserRole != "ADMIN" && currentUserId != createDto.CustomerId)
                    {
                        return Forbid();
                    }
                }

                var booking = await _bookingService.CreateBookingDtoAsync(createDto);

                // Notify admins via SignalR
                await _bookingHubContext.Clients.Group("AdminBookingMonitoring")
                    .SendAsync("NewBookingCreated", booking);

                return CreatedAtAction(nameof(GetBookingById), new { id = booking.BookingId }, booking);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = "Invalid operation", message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "Internal server error",
                    message = "An error occurred while creating the booking",
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Update a booking
        /// </summary>
        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult<BookingDto>> UpdateBooking(int id, [FromBody] UpdateBookingDto updateDto)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { error = "Invalid booking ID", message = "Booking ID must be a positive number" });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        error = "Validation failed",
                        message = "Please check your input data",
                        details = ModelState.Where(x => x.Value!.Errors.Count > 0)
                            .ToDictionary(
                                kvp => kvp.Key,
                                kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                            )
                    });
                }

                // Get existing booking to check authorization
                var bookings = await _bookingService.GetAllBookingsAsync();
                var existingBooking = bookings.FirstOrDefault(b => b.BookingId == id);

                if (existingBooking == null)
                {
                    return NotFound(new { error = "Booking not found", message = $"Booking with ID {id} does not exist" });
                }

                // Check authorization
                var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

                if (currentUserIdClaim != null && int.TryParse(currentUserIdClaim.Value, out int currentUserId))
                {
                    if (currentUserRole != "ADMIN" && currentUserId != existingBooking.CustomerId)
                    {
                        return Forbid();
                    }
                }

                var booking = await _bookingService.UpdateBookingAsync(id, updateDto);
                if (booking == null)
                {
                    return NotFound(new { error = "Booking not found", message = $"Booking with ID {id} does not exist" });
                }

                // Notify specific booking room
                await _bookingHubContext.Clients.Group($"Booking_{id}")
                    .SendAsync("BookingUpdated", booking);

                return Ok(booking);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "Internal server error",
                    message = "An error occurred while updating the booking",
                    details = ex.Message
                });
            }
        }

        [HttpPost("{id}/photoLink")]
        [Authorize]
        public async Task<ActionResult<BookingDto>> UpdatePhotoLink(int id, PhotoLinkUpdateDto photoLink)
        {
            try
            {
                var updatePhotoLink = await _bookingService.UpdatePhotoLinkAsync(id, photoLink);


                if (updatePhotoLink == null)
                {
                    return NotFound(new { error = "Booking not found", message = $"Booking with ID {id} does not exist" });
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { error = "Unauthorized", message = "User identity not found" });
                }

                var notification = new PhotoLinkUpdateNotificationDto
                {
                    BookingId = updatePhotoLink.BookingId,
                    PhotographerId = int.Parse(userId),
                    CustomerId = updatePhotoLink.CustomerId,
                    PhotoLink = photoLink.PhotoLink,
                    UpdatedAt = DateTime.UtcNow
                };

                await _bookingHubContext.Clients.Group("AdminBookingMonitoring")
                    .SendAsync("BookingPhotoLinkChanged", notification);

                return Ok(updatePhotoLink);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "Internal server error",
                    message = "An error occurred while updating the photo link",
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Update booking status (Admin or Photographer can update)
        /// </summary>
        [HttpPatch("{id}/status")]
        [Authorize]
        public async Task<ActionResult<BookingDto>> UpdateBookingStatus(int id, [FromBody] UpdateBookingStatusDto statusDto)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { error = "Invalid booking ID", message = "Booking ID must be a positive number" });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        error = "Validation failed",
                        message = "Please check your input data",
                        details = ModelState.Where(x => x.Value!.Errors.Count > 0)
                            .ToDictionary(
                                kvp => kvp.Key,
                                kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                            )
                    });
                }

                // Get existing booking to check authorization
                var bookings = await _bookingService.GetAllBookingsAsync();
                var existingBooking = bookings.FirstOrDefault(b => b.BookingId == id);

                if (existingBooking == null)
                {
                    return NotFound(new { error = "Booking not found", message = $"Booking with ID {id} does not exist" });
                }

                // Check authorization - Admin or Photographer can update status
                var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

                if (currentUserIdClaim != null && int.TryParse(currentUserIdClaim.Value, out int currentUserId))
                {
                    if (currentUserRole != "ADMIN" && currentUserId != existingBooking.PhotographerId)
                    {
                        return Forbid();
                    }
                }

                var booking = await _bookingService.UpdateBookingStatusDtoAsync(id, statusDto);
                if (booking == null)
                {
                    return NotFound(new { error = "Booking not found", message = $"Booking with ID {id} does not exist" });
                }

                // Create notification
                var notification = new BookingStatusNotificationDto
                {
                    BookingId = booking.BookingId,
                    OldStatusId = existingBooking.StatusId,
                    OldStatusName = existingBooking.StatusName,
                    NewStatusId = booking.StatusId ?? 0,
                    NewStatusName = booking.StatusName ?? "Unknown",
                    ChangedAt = DateTime.UtcNow,
                    CustomerName = booking.CustomerName,
                    PhotographerName = booking.PhotographerName,
                    ScheduleAt = booking.ScheduleAt,
                    Price = booking.Price
                };

                // Notify admins via SignalR
                await _bookingHubContext.Clients.Group("AdminBookingMonitoring")
                    .SendAsync("BookingStatusChanged", notification);

                // Notify specific booking room
                await _bookingHubContext.Clients.Group($"Booking_{id}")
                    .SendAsync("BookingStatusChanged", notification);

                return Ok(booking);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "Internal server error",
                    message = "An error occurred while updating the booking status",
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Delete a booking (Admin only)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult> DeleteBooking(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { error = "Invalid booking ID", message = "Booking ID must be a positive number" });
                }

                var result = await _bookingService.DeleteBookingAsync(id);
                if (!result)
                {
                    return NotFound(new { error = "Booking not found", message = $"Booking with ID {id} does not exist" });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "Internal server error",
                    message = "An error occurred while deleting the booking",
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Get bookings count by status (Admin only) - Legacy version
        /// </summary>
        [HttpGet("statistics/count-by-status")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<Dictionary<int, int>>> GetBookingsCountByStatus()
        {
            try
            {
                var counts = await _bookingService.GetBookingsCountByStatusAsync();
                return Ok(counts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "Internal server error",
                    message = "An error occurred while retrieving booking statistics",
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Get detailed booking statistics with status names and percentages (Admin only)
        /// </summary>
        [HttpGet("statistics/detailed")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<BookingStatisticsResponseDto>> GetDetailedBookingStatistics()
        {
            try
            {
                var statistics = await _bookingService.GetBookingStatisticsAsync();
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "Internal server error",
                    message = "An error occurred while retrieving detailed booking statistics",
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Legacy methods for backward compatibility
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("legacy")]
        public async Task<IActionResult> CreateBookingLegacy([FromBody] CreateBookingRequest request)
        {
            var result = await _bookingService.CreateBookingAsync(request);
            return CreatedAtAction(nameof(GetBookingById), new { id = result.BookingId }, result);
        }

        [HttpPut("{id}/status/legacy")]
        public async Task<IActionResult> UpdateBookingStatusLegacy(int id, [FromBody] int statusId)
        {
            var result = await _bookingService.UpdateBookingStatusAsync(id, statusId);
            return Ok(result);
        }

        // GET api/booking/me - get bookings for current user from JWT with pagination
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetMyBookings([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userIdValue) || !int.TryParse(userIdValue, out var userId))
            {
                return Unauthorized("Invalid user identity");
            }

            var bookings = await _bookingService.GetMyBookingsAsync(userId, page, pageSize);
            return Ok(bookings);
        }

        /// <summary>
        /// Get pending bookings for photographer (Photographer only)
        /// </summary>
        /// <remarks>
        /// Returns a paginated list of pending bookings for the specified photographer.
        /// Response includes minimal user information, scheduling details, pricing, and photo type.
        /// </remarks>
        /// <param name="photographerId">ID of the photographer</param>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Number of items per page (default: 10, max: 100)</param>
        [HttpGet("photographer/{photographerId}/pending")]
        [Authorize]
        public async Task<ActionResult<PhotographerPendingBookingsResponseDto>> GetPhotographerPendingBookings(
     int photographerId,
   [FromQuery] int page = 1,
             [FromQuery] int pageSize = 10)
        {
            try
            {
                if (photographerId <= 0)
                {
                    return BadRequest(new
                    {
                        error = "Invalid photographer ID",
                        message = "Photographer ID must be a positive number"
                    });
                }

                // Check authorization - photographer can only view their own pending bookings
                var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

                if (currentUserIdClaim != null && int.TryParse(currentUserIdClaim.Value, out int currentUserId))
                {
                    if (currentUserRole != "ADMIN" && currentUserId != photographerId)
                    {
                        return Forbid();
                    }
                }

                var result = await _bookingService.GetPhotographerPendingBookingsAsync(photographerId, page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "Internal server error",
                    message = "An error occurred while retrieving pending bookings",
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Get current user's bookings filtered by status
        /// </summary>
        [HttpGet("me/status/{statusId}")]
        [Authorize]
        public async Task<ActionResult<PhotographerPendingBookingsResponseDto>> GetMyBookingsByStatus(
     int statusId,
      [FromQuery] int page = 1,
     [FromQuery] int pageSize = 10)
        {
            try
            {
                if (statusId <= 0)
                {
                    return BadRequest(new { error = "Invalid status ID", message = "Status ID must be a positive number" });
                }

                var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrWhiteSpace(userIdValue) || !int.TryParse(userIdValue, out var userId))
                {
                    return Unauthorized(new { error = "Unauthorized", message = "Invalid user identity" });
                }

                var result = await _bookingService.GetUserBookingsByStatusAsync(userId, statusId, page, pageSize);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "Internal server error",
                    message = "An error occurred while retrieving bookings by status",
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Get current user's bookings filtered by multiple statuses
        /// </summary>
        [HttpPost("me/statuses")]
        [Authorize]
        public async Task<ActionResult<PhotographerPendingBookingsResponseDto>> GetMyBookingsByMultipleStatuses(
            [FromBody] GetBookingsByMultipleStatusesDto request)
        {
            try
            {
                if (request == null || request.StatusIds == null || request.StatusIds.Count == 0)
                {
                    return BadRequest(new
                    {
                        error = "Invalid request",
                        message = "At least one status ID must be provided"
                    });
                }

                if (request.StatusIds.Any(id => id <= 0))
                {
                    return BadRequest(new
                    {
                        error = "Invalid status IDs",
                        message = "All status IDs must be positive numbers"
                    });
                }

                var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrWhiteSpace(userIdValue) || !int.TryParse(userIdValue, out var userId))
                {
                    return Unauthorized(new { error = "Unauthorized", message = "Invalid user identity" });
                }

                var page = Math.Max(1, request.Page ?? 1);
                var pageSize = Math.Clamp(request.PageSize ?? 10, 1, 100);

                var result = await _bookingService.GetUserBookingsByMultipleStatusesAsync(
      userId,
                  request.StatusIds,
              page,
             pageSize);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "Internal server error",
                    message = "An error occurred while retrieving bookings by multiple statuses",
                    details = ex.Message
                });
            }
        }
    }
}
