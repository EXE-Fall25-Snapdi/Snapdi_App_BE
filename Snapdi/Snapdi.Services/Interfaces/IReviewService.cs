using Snapdi.Services.DTOs;

namespace Snapdi.Services.Interfaces
{
    public interface IReviewService
    {
        /// <summary>
        /// Create a new review for a booking
        /// </summary>
        Task<ReviewDto> CreateReviewAsync(int userId, CreateReviewDto createReviewDto);

        /// <summary>
        /// Get review by ID
        /// </summary>
        Task<ReviewDto?> GetReviewByIdAsync(int reviewId);

        /// <summary>
        /// Get review by booking ID
        /// </summary>
        Task<ReviewDto?> GetReviewByBookingIdAsync(int bookingId);

        /// <summary>
        /// Get all reviews with pagination
        /// </summary>
        Task<PagedResultDto<ReviewDto>> GetReviewsPagedAsync(int page, int pageSize);

        /// <summary>
        /// Get reviews by user ID
        /// </summary>
        Task<IEnumerable<ReviewDto>> GetReviewsByUserIdAsync(int userId);

        /// <summary>
        /// Update a review
        /// </summary>
        Task<ReviewDto?> UpdateReviewAsync(int reviewId, int userId, UpdateReviewDto updateReviewDto);

        /// <summary>
        /// Delete a review
        /// </summary>
        Task<bool> DeleteReviewAsync(int reviewId, int userId);

        /// <summary>
        /// Get review statistics
        /// </summary>
        Task<ReviewStatisticsDto> GetReviewStatisticsAsync();
    }
}
