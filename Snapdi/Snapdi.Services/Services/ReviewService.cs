using Snapdi.Repositories.Interfaces;
using Snapdi.Repositories.Models;
using Snapdi.Services.DTOs;
using Snapdi.Services.Interfaces;

namespace Snapdi.Services.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly IBookingRepository _bookingRepository;

        public ReviewService(
            IReviewRepository reviewRepository,
            IBookingRepository bookingRepository)
        {
            _reviewRepository = reviewRepository;
            _bookingRepository = bookingRepository;
        }

        public async Task<ReviewDto> CreateReviewAsync(int userId, CreateReviewDto createReviewDto)
        {
            // Check if booking exists
            var booking = await _bookingRepository.GetBookingWithDetailsAsync(createReviewDto.BookingId);
            if (booking == null)
            {
                throw new InvalidOperationException("Booking not found");
            }

            // Check if booking belongs to the user (as customer)
            if (booking.CustomerId != userId)
            {
                throw new UnauthorizedAccessException("You can only review your own bookings");
            }

            // Check if booking status is "done"
            if (booking.Status == null || booking.Status.StatusName.ToLower() != "completed")
            {
                throw new InvalidOperationException("You can only review completed bookings");
            }

            // Check if booking already has a review
            if (await _reviewRepository.HasReviewForBookingAsync(createReviewDto.BookingId))
            {
                throw new InvalidOperationException("This booking has already been reviewed");
            }

            // Create review
            var review = new Review
            {
                BookingId = createReviewDto.BookingId,
                FromUserId = userId,
                Rating = createReviewDto.Rating,
                Comment = createReviewDto.Comment,
                CreateAt = DateTime.UtcNow
            };

            var createdReview = await _reviewRepository.AddAsync(review);
            await _reviewRepository.SaveChangesAsync();

            // Reload with navigation properties
            var reviewWithDetails = await _reviewRepository.GetByIdAsync(createdReview.ReviewId);

            return MapToReviewDto(reviewWithDetails!);
        }

        public async Task<ReviewDto?> GetReviewByIdAsync(int reviewId)
        {
            var review = await _reviewRepository.GetByIdAsync(reviewId);
            return review != null ? MapToReviewDto(review) : null;
        }

        public async Task<ReviewDto?> GetReviewByBookingIdAsync(int bookingId)
        {
            var review = await _reviewRepository.GetReviewByBookingIdAsync(bookingId);
            return review != null ? MapToReviewDto(review) : null;
        }

        public async Task<PagedResultDto<ReviewDto>> GetReviewsPagedAsync(int page, int pageSize)
        {
            var (reviews, totalCount) = await _reviewRepository.GetReviewsPagedAsync(page, pageSize);
            var reviewDtos = reviews.Select(MapToReviewDto).ToList();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            return new PagedResultDto<ReviewDto>
            {
                Items = reviewDtos,
                CurrentPage = page,
                PageSize = pageSize,
                TotalItems = totalCount,
                TotalPages = totalPages
            };
        }

        public async Task<IEnumerable<ReviewDto>> GetReviewsByUserIdAsync(int userId)
        {
            var reviews = await _reviewRepository.GetReviewsByUserIdAsync(userId);
            return reviews.Select(MapToReviewDto);
        }

        public async Task<ReviewDto?> UpdateReviewAsync(int reviewId, int userId, UpdateReviewDto updateReviewDto)
        {
            var review = await _reviewRepository.GetByIdAsync(reviewId);
            if (review == null)
            {
                return null;
            }

            // Check if user owns the review
            if (review.FromUserId != userId)
            {
                throw new UnauthorizedAccessException("You can only update your own reviews");
            }

            // Update review
            review.Rating = updateReviewDto.Rating;
            review.Comment = updateReviewDto.Comment;

            await _reviewRepository.UpdateAsync(review);
            await _reviewRepository.SaveChangesAsync();

            // Reload with navigation properties
            var updatedReview = await _reviewRepository.GetByIdAsync(reviewId);
            return MapToReviewDto(updatedReview!);
        }

        public async Task<bool> DeleteReviewAsync(int reviewId, int userId)
        {
            var review = await _reviewRepository.GetByIdAsync(reviewId);
            if (review == null)
            {
                return false;
            }

            // Check if user owns the review
            if (review.FromUserId != userId)
            {
                throw new UnauthorizedAccessException("You can only delete your own reviews");
            }

            await _reviewRepository.DeleteAsync(review);
            await _reviewRepository.SaveChangesAsync();
            return true;
        }

        public async Task<ReviewStatisticsDto> GetReviewStatisticsAsync()
        {
            var totalReviews = await _reviewRepository.GetTotalReviewCountAsync();
            var averageRating = await _reviewRepository.GetAverageRatingAsync();
            var ratingCounts = await _reviewRepository.GetReviewCountByRatingAsync();

            return new ReviewStatisticsDto
            {
                TotalReviews = totalReviews,
                AverageRating = Math.Round(averageRating, 2),
                FiveStarCount = ratingCounts.GetValueOrDefault(5, 0),
                FourStarCount = ratingCounts.GetValueOrDefault(4, 0),
                ThreeStarCount = ratingCounts.GetValueOrDefault(3, 0),
                TwoStarCount = ratingCounts.GetValueOrDefault(2, 0),
                OneStarCount = ratingCounts.GetValueOrDefault(1, 0)
            };
        }

        private static ReviewDto MapToReviewDto(Review review)
        {
            return new ReviewDto
            {
                ReviewId = review.ReviewId,
                BookingId = review.BookingId,
                FromUserId = review.FromUserId,
                FromUserName = review.FromUser?.Name,
                FromUserAvatar = review.FromUser?.AvatarUrl,
                Rating = review.Rating,
                Comment = review.Comment,
                CreateAt = review.CreateAt
            };
        }
    }
}
