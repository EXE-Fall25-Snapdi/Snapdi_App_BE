using Microsoft.EntityFrameworkCore;
using Snapdi.Repositories.Context;
using Snapdi.Repositories.Interfaces;
using Snapdi.Repositories.Models;

namespace Snapdi.Repositories.Repositories
{
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        public UserRepository(SnapdiDbV2Context context) : base(context)
        {
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
        }

        public async Task<User?> GetByPhoneAsync(string phone)
        {
            return await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Phone == phone);
        }

        public async Task<User?> GetByEmailOrPhoneAsync(string emailOrPhone)
        {
            return await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email.ToLower() == emailOrPhone.ToLower() || u.Phone == emailOrPhone);
        }

        public async Task<User?> GetByRefreshTokenAsync(string refreshToken)
        {
            return await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken && u.ExpiredRefreshTokenAt > DateTime.UtcNow);
        }

        public async Task<User?> GetByEmailVerificationTokenAsync(string verificationToken)
        {
            // For simplicity, we'll use a basic implementation
            // In production, you might want to store verification tokens in a separate table
            return await _context.Users
                .FirstOrDefaultAsync(u => u.RefreshToken == verificationToken);
        }

        public async Task<IEnumerable<User>> GetUsersByRoleAsync(int roleId)
        {
            return await _context.Users
                .Include(u => u.Role)
                .Where(u => u.RoleId == roleId)
                .ToListAsync();
        }

        public async Task<IEnumerable<User>> GetActiveUsersAsync()
        {
            return await _context.Users
                .Include(u => u.Role)
                .Where(u => u.IsActive)
                .ToListAsync();
        }

        public async Task<IEnumerable<User>> GetVerifiedUsersAsync()
        {
            return await _context.Users
                .Include(u => u.Role)
                .Where(u => u.IsVerify)
                .ToListAsync();
        }

        public async Task<User?> GetUserWithRoleAsync(int userId)
        {
            return await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == userId);
        }

        public async Task<User?> GetUserWithPhotographerProfileAsync(int userId)
        {
            return await _context.Users
                .Include(u => u.Role)
                .Include(u => u.PhotographerProfile)
                .Include(u => u.PhotoPortfolios)
                .FirstOrDefaultAsync(u => u.UserId == userId);
        }

        public async Task<bool> IsEmailExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email.ToLower() == email.ToLower());
        }

        public async Task<bool> IsPhoneExistsAsync(string phone)
        {
            return await _context.Users.AnyAsync(u => u.Phone == phone);
        }

        public async Task UpdateRefreshTokenAsync(int userId, string refreshToken, DateTime expiredAt)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.RefreshToken = refreshToken;
                user.ExpiredRefreshTokenAt = expiredAt;
            }
        }

        public async Task UpdatePasswordAsync(int userId, string newPassword)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.Password = newPassword;
            }
        }

        public async Task UpdateUserStatusAsync(int userId, bool isActive, bool isVerify)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.IsActive = isActive;
                user.IsVerify = isVerify;
            }
        }

        public async Task UpdateEmailVerificationTokenAsync(int userId, string verificationToken, DateTime expiredAt)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                // For simplicity, storing in RefreshToken field temporarily
                // In production, you'd want a separate verification token field
                user.RefreshToken = verificationToken;
                user.ExpiredRefreshTokenAt = expiredAt;
            }
        }

        public async Task VerifyEmailAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.IsVerify = true;
                user.RefreshToken = string.Empty; // Clear verification token
            }
        }

        public async Task<(IEnumerable<User> Users, int TotalCount)> GetUsersWithFilterAsync(
            int page, 
            int pageSize, 
            string? searchTerm = null,
            int? roleId = null,
            bool? isActive = null,
            bool? isVerified = null,
            string? locationCity = null,
            string? sortBy = null,
            string? sortDirection = "asc",
            DateTime? createdFrom = null,
            DateTime? createdTo = null)
        {
            var query = _context.Users.Include(u => u.Role).AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(searchTerm))
            {
                var searchLower = searchTerm.ToLower();
                query = query.Where(u => 
                    u.Name.ToLower().Contains(searchLower) || 
                    u.Email.ToLower().Contains(searchLower));
            }

            if (roleId.HasValue)
            {
                query = query.Where(u => u.RoleId == roleId.Value);
            }

            if (isActive.HasValue)
            {
                query = query.Where(u => u.IsActive == isActive.Value);
            }

            if (isVerified.HasValue)
            {
                query = query.Where(u => u.IsVerify == isVerified.Value);
            }

            if (!string.IsNullOrEmpty(locationCity))
            {
                query = query.Where(u => u.LocationCity != null && u.LocationCity.ToLower().Contains(locationCity.ToLower()));
            }

            if (createdFrom.HasValue)
            {
                query = query.Where(u => u.CreatedAt >= createdFrom.Value);
            }

            if (createdTo.HasValue)
            {
                query = query.Where(u => u.CreatedAt <= createdTo.Value);
            }

            // Apply sorting
            if (!string.IsNullOrEmpty(sortBy))
            {
                var isDescending = sortDirection?.ToLower() == "desc";
                
                query = sortBy.ToLower() switch
                {
                    "name" => isDescending ? query.OrderByDescending(u => u.Name) : query.OrderBy(u => u.Name),
                    "email" => isDescending ? query.OrderByDescending(u => u.Email) : query.OrderBy(u => u.Email),
                    "createdat" => isDescending ? query.OrderByDescending(u => u.CreatedAt) : query.OrderBy(u => u.CreatedAt),
                    _ => query.OrderBy(u => u.UserId)
                };
            }
            else
            {
                query = query.OrderBy(u => u.UserId);
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync();

            // Apply pagination
            var users = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (users, totalCount);
        }

        public override async Task<User?> GetByIdAsync(int id)
        {
            return await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == id);
        }

        public override async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _context.Users
                .Include(u => u.Role)
                .ToListAsync();
        }
    }
}