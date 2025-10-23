using Microsoft.EntityFrameworkCore;
using Snapdi.Repositories.Context;
using Snapdi.Repositories.Interfaces;
using Snapdi.Repositories.Models;

namespace Snapdi.Repositories.Repositories
{
    public class PhotographerProfileRepository : BaseRepository<PhotographerProfile>, IPhotographerProfileRepository
    {
        public PhotographerProfileRepository(SnapdiDbV2Context context) : base(context)
        {
        }

        public async Task<PhotographerProfile?> GetByUserIdAsync(int userId)
        {
            return await _context.PhotographerProfiles
                .Include(p => p.User)
                .ThenInclude(u => u!.Role)
                .FirstOrDefaultAsync(p => p.UserId == userId);
        }

        public async Task<IEnumerable<PhotographerProfile>> GetAvailablePhotographersAsync()
        {
            return await _context.PhotographerProfiles
                .Include(p => p.User)
                .ThenInclude(u => u!.Role)
                .Where(p => p.IsAvailable && p.User.IsActive && p.User.IsVerify)
                .ToListAsync();
        }

        public async Task<IEnumerable<PhotographerProfile>> GetPhotographersByCityAsync(string city)
        {
            return await _context.PhotographerProfiles
                .Include(p => p.User)
                .ThenInclude(u => u!.Role)
                .Where(p => p.User.LocationCity != null && 
                           p.User.LocationCity.ToLower().Contains(city.ToLower()) &&
                           p.User.IsActive && p.User.IsVerify)
                .ToListAsync();
        }

        public async Task<bool> ExistsByUserIdAsync(int userId)
        {
            return await _context.PhotographerProfiles.AnyAsync(p => p.UserId == userId);
        }

        public async Task UpdatePhotographerStatusAsync(int userId, bool isAvailable)
        {
            var photographerProfile = await _context.PhotographerProfiles.FindAsync(userId);
            if (photographerProfile != null)
            {
                photographerProfile.IsAvailable = isAvailable;
            }
        }

        public override async Task<PhotographerProfile?> GetByIdAsync(int id)
        {
            return await _context.PhotographerProfiles
                .Include(p => p.User)
                .ThenInclude(u => u!.Role)
                .FirstOrDefaultAsync(p => p.UserId == id);
        }

        public override async Task<IEnumerable<PhotographerProfile>> GetAllAsync()
        {
            return await _context.PhotographerProfiles
                .Include(p => p.User)
                .ThenInclude(u => u!.Role)
                .ToListAsync();
        }
    }
}