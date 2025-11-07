using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Snapdi.Services.DTOs;
using Snapdi.Services.Interfaces;
using System.Security.Claims;

namespace Snapdi.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewsController : ControllerBase
    {
        private readonly IReviewService _reviewService;
        private readonly ILogger<ReviewsController> _logger;

        public ReviewsController(
            IReviewService reviewService,
            ILogger<ReviewsController> logger)
        {
            _reviewService = reviewService;
            _logger = logger;
        }

        /// <summary>
        /// Create a new review for a booking
        /// Only customers can create reviews for their completed bookings (status = done)
        /// </summary>
        /// <param name="createReviewDto">Review data</param>
        /// <returns>Created review</returns>
        [HttpPost]
        [Authorize] // Any authenticated user
        public async Task<ActionResult<ReviewDto>> CreateReview([FromBody] CreateReviewDto createReviewDto)
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

                // Get user ID from JWT token
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return BadRequest(new { error = "Invalid token", message = "User ID not found in token claims" });
                }

                var review = await _reviewService.CreateReviewAsync(userId, createReviewDto);
                return CreatedAtAction(nameof(GetReviewById), new { id = review.ReviewId }, review);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = "Invalid operation", message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating review");
                return StatusCode(500, new
                {
                    error = "Internal server error",
                    message = "An error occurred while creating the review",
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Get review by ID
        /// </summary>
        /// <param name="id">Review ID</param>
        /// <returns>Review details</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<ReviewDto>> GetReviewById(int id)
        {
            try
            {
                var review = await _reviewService.GetReviewByIdAsync(id);
                if (review == null)
                {
                    return NotFound(new { error = "Review not found", message = $"Review with ID {id} not found" });
                }

                return Ok(review);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving review {ReviewId}", id);
                return StatusCode(500, new
                {
                    error = "Internal server error",
                    message = "An error occurred while retrieving the review",
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Get review by booking ID
        /// </summary>
        /// <param name="bookingId">Booking ID</param>
        /// <returns>Review for the booking</returns>
        [HttpGet("booking/{bookingId}")]
        public async Task<ActionResult<ReviewDto>> GetReviewByBookingId(int bookingId)
        {
            try
            {
                var review = await _reviewService.GetReviewByBookingIdAsync(bookingId);
                if (review == null)
                {
                    return NotFound(new { error = "Review not found", message = $"No review found for booking {bookingId}" });
                }

                return Ok(review);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving review for booking {BookingId}", bookingId);
                return StatusCode(500, new
                {
                    error = "Internal server error",
                    message = "An error occurred while retrieving the review",
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Get all reviews with pagination
        /// </summary>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10, max: 50)</param>
        /// <returns>Paginated list of reviews</returns>
        [HttpGet]
        public async Task<ActionResult<PagedResultDto<ReviewDto>>> GetReviewsPaged(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 50) pageSize = 10;

                var result = await _reviewService.GetReviewsPagedAsync(page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reviews");
                return StatusCode(500, new
                {
                    error = "Internal server error",
                    message = "An error occurred while retrieving reviews",
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Get reviews by user ID (reviews created by the user)
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>List of reviews</returns>
        [HttpGet("user/{userId}")]
        [Authorize] // Any authenticated user
        public async Task<ActionResult<IEnumerable<ReviewDto>>> GetReviewsByUserId(int userId)
        {
            try
            {
                var reviews = await _reviewService.GetReviewsByUserIdAsync(userId);
                return Ok(reviews);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reviews for user {UserId}", userId);
                return StatusCode(500, new
                {
                    error = "Internal server error",
                    message = "An error occurred while retrieving reviews",
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Update a review
        /// Users can only update their own reviews
        /// </summary>
        /// <param name="id">Review ID</param>
        /// <param name="updateReviewDto">Updated review data</param>
        /// <returns>Updated review</returns>
        [HttpPut("{id}")]
        [Authorize] // Any authenticated user
        public async Task<ActionResult<ReviewDto>> UpdateReview(int id, [FromBody] UpdateReviewDto updateReviewDto)
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

                // Get user ID from JWT token
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return BadRequest(new { error = "Invalid token", message = "User ID not found in token claims" });
                }

                var review = await _reviewService.UpdateReviewAsync(id, userId, updateReviewDto);
                if (review == null)
                {
                    return NotFound(new { error = "Review not found", message = $"Review with ID {id} not found" });
                }

                return Ok(review);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating review {ReviewId}", id);
                return StatusCode(500, new
                {
                    error = "Internal server error",
                    message = "An error occurred while updating the review",
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Delete a review
        /// Users can only delete their own reviews, Admins can delete any review
        /// </summary>
        /// <param name="id">Review ID</param>
        /// <returns>No content</returns>
        [HttpDelete("{id}")]
        [Authorize] // Any authenticated user
        public async Task<ActionResult> DeleteReview(int id)
        {
            try
            {
                // Get user ID from JWT token
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return BadRequest(new { error = "Invalid token", message = "User ID not found in token claims" });
                }

                var result = await _reviewService.DeleteReviewAsync(id, userId);
                if (!result)
                {
                    return NotFound(new { error = "Review not found", message = $"Review with ID {id} not found" });
                }

                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting review {ReviewId}", id);
                return StatusCode(500, new
                {
                    error = "Internal server error",
                    message = "An error occurred while deleting the review",
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Get review statistics
        /// </summary>
        /// <returns>Review statistics including total count, average rating, and star distribution</returns>
        [HttpGet("statistics")]
        public async Task<ActionResult<ReviewStatisticsDto>> GetReviewStatistics()
        {
            try
            {
                var statistics = await _reviewService.GetReviewStatisticsAsync();
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving review statistics");
                return StatusCode(500, new
                {
                    error = "Internal server error",
                    message = "An error occurred while retrieving review statistics",
                    details = ex.Message
                });
            }
        }
    }
}
