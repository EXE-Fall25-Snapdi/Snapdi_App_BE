using Microsoft.EntityFrameworkCore;
using Snapdi.Repositories.Context;
using Snapdi.Repositories.Interfaces;
using Snapdi.Repositories.Models;

namespace Snapdi.Repositories.Repositories
{
    public class PhotoPortfolioRepository : BaseRepository<PhotoPortfolio>, IPhotoPortfolioRepository
    {
        public PhotoPortfolioRepository(SnapdiDbV2Context context) : base(context)
        {
        }

        public async Task<IEnumerable<PhotoPortfolio>> GetByUserIdAsync(int userId)
        {
            return await _context.PhotoPortfolios
                .Where(p => p.UserId == userId)
                .OrderBy(p => p.PhotoPortfolioId)
                .ToListAsync();
        }

        public async Task<PhotoPortfolio?> GetByIdAndUserIdAsync(int photoPortfolioId, int userId)
        {
            return await _context.PhotoPortfolios
                .FirstOrDefaultAsync(p => p.PhotoPortfolioId == photoPortfolioId && p.UserId == userId);
        }

        public async Task<bool> ExistsByUserIdAsync(int userId)
        {
            return await _context.PhotoPortfolios.AnyAsync(p => p.UserId == userId);
        }

        public async Task<int> CountByUserIdAsync(int userId)
        {
            return await _context.PhotoPortfolios.CountAsync(p => p.UserId == userId);
        }

        public async Task DeleteAllByUserIdAsync(int userId)
        {
            var portfolios = await _context.PhotoPortfolios
                .Where(p => p.UserId == userId)
                .ToListAsync();

            if (portfolios.Any())
            {
                _context.PhotoPortfolios.RemoveRange(portfolios);
            }
        }
    }
}