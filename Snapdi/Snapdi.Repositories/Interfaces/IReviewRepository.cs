using Snapdi.Repositories.Models;

namespace Snapdi.Repositories.Interfaces
{
    public interface IReviewRepository : IBaseRepository<Review>
    {
        /// <summary>
        /// Get review by booking ID
        /// </summary>
        Task<Review?> GetReviewByBookingIdAsync(int bookingId);

        /// <summary>
        /// Get all reviews with pagination
        /// </summary>
        Task<(IEnumerable<Review> Reviews, int TotalCount)> GetReviewsPagedAsync(int page, int pageSize);

        /// <summary>
        /// Get reviews by user ID (reviews created by user)
        /// </summary>
        Task<IEnumerable<Review>> GetReviewsByUserIdAsync(int userId);

        /// <summary>
        /// Get total review count
        /// </summary>
        Task<int> GetTotalReviewCountAsync();

        /// <summary>
        /// Get average rating
        /// </summary>
        Task<double> GetAverageRatingAsync();

        /// <summary>
        /// Get review count by rating
        /// </summary>
        Task<Dictionary<int, int>> GetReviewCountByRatingAsync();

        /// <summary>
        /// Check if booking already has a review
        /// </summary>
        Task<bool> HasReviewForBookingAsync(int bookingId);
    }
}
