using Microsoft.EntityFrameworkCore;
using Snapdi.Repositories.Context;
using Snapdi.Repositories.Interfaces;
using Snapdi.Repositories.Models;

namespace Snapdi.Repositories.Repositories
{
    public class ReviewRepository : BaseRepository<Review>, IReviewRepository
    {
        public ReviewRepository(SnapdiDbV2Context context) : base(context)
        {
        }

        public async Task<Review?> GetReviewByBookingIdAsync(int bookingId)
        {
            return await _context.Reviews
                .Include(r => r.FromUser)
                .Include(r => r.Booking)
                .FirstOrDefaultAsync(r => r.BookingId == bookingId);
        }

        public async Task<(IEnumerable<Review> Reviews, int TotalCount)> GetReviewsPagedAsync(int page, int pageSize)
        {
            var query = _context.Reviews
                .Include(r => r.FromUser)
                .Include(r => r.Booking)
                .OrderByDescending(r => r.CreateAt);

            var totalCount = await query.CountAsync();

            var reviews = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (reviews, totalCount);
        }

        public async Task<IEnumerable<Review>> GetReviewsByUserIdAsync(int userId)
        {
            return await _context.Reviews
                .Include(r => r.FromUser)
                .Include(r => r.Booking)
                .Where(r => r.FromUserId == userId)
                .OrderByDescending(r => r.CreateAt)
                .ToListAsync();
        }

        public async Task<int> GetTotalReviewCountAsync()
        {
            return await _context.Reviews.CountAsync();
        }

        public async Task<double> GetAverageRatingAsync()
        {
            if (!await _context.Reviews.AnyAsync())
                return 0;

            return await _context.Reviews.AverageAsync(r => r.Rating);
        }

        public async Task<Dictionary<int, int>> GetReviewCountByRatingAsync()
        {
            var reviews = await _context.Reviews.ToListAsync();
            
            var ratingCounts = new Dictionary<int, int>();
            for (int i = 1; i <= 5; i++)
            {
                ratingCounts[i] = reviews.Count(r => Math.Floor(r.Rating) == i);
            }

            return ratingCounts;
        }

        public async Task<bool> HasReviewForBookingAsync(int bookingId)
        {
            return await _context.Reviews.AnyAsync(r => r.BookingId == bookingId);
        }

        public override async Task<Review?> GetByIdAsync(int id)
        {
            return await _context.Reviews
                .Include(r => r.FromUser)
                .Include(r => r.Booking)
                .FirstOrDefaultAsync(r => r.ReviewId == id);
        }

        public override async Task<IEnumerable<Review>> GetAllAsync()
        {
            return await _context.Reviews
                .Include(r => r.FromUser)
                .Include(r => r.Booking)
                .OrderByDescending(r => r.CreateAt)
                .ToListAsync();
        }
    }
}
