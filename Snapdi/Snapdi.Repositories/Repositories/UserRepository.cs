using Microsoft.EntityFrameworkCore;
using Snapdi.Repositories.Data;
using Snapdi.Repositories.Interfaces;
using Snapdi.Repositories.Models;

namespace Snapdi.Repositories.Repositories
{
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        public UserRepository(SnapdiDbV3Context context) : base(context)
        {
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _dbSet
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User?> GetByPhoneAsync(string phone)
        {
            return await _dbSet
                .FirstOrDefaultAsync(u => u.Phone == phone);
        }

        public async Task<User?> GetByEmailOrPhoneAsync(string emailOrPhone)
        {
            return await _dbSet
                .FirstOrDefaultAsync(u => u.Email == emailOrPhone || u.Phone == emailOrPhone);
        }

        public async Task<User?> GetByRefreshTokenAsync(string refreshToken)
        {
            return await _dbSet
                .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken &&
                                         u.ExpiredRefreshTokenAt > DateTime.UtcNow);
        }

        public async Task<User?> GetByEmailVerificationTokenAsync(string verificationToken)
        {
            return await _dbSet
                .FirstOrDefaultAsync(u => u.RefreshToken == verificationToken &&
                                         u.ExpiredRefreshTokenAt > DateTime.UtcNow);
        }

        public async Task<IEnumerable<User>> GetUsersByRoleAsync(int roleId)
        {
            return await _dbSet
                .Where(u => u.RoleId == roleId)
                .ToListAsync();
        }

        public async Task<IEnumerable<User>> GetActiveUsersAsync()
        {
            return await _dbSet
                .Where(u => u.IsActive)
                .ToListAsync();
        }

        public async Task<IEnumerable<User>> GetVerifiedUsersAsync()
        {
            return await _dbSet
                .Where(u => u.IsVerify)
                .ToListAsync();
        }

        public async Task<User?> GetUserWithRoleAsync(int userId)
        {
            return await _dbSet
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == userId);
        }

        public async Task<User?> GetUserWithPhotographerProfileAsync(int userId)
        {
            return await _dbSet
                .Include(u => u.PhotographerProfile)
                .ThenInclude(p => p.Equipment)
                .Include(u => u.PhotographerProfile)
                .ThenInclude(p => p.PhotoPortfolio)
                .FirstOrDefaultAsync(u => u.UserId == userId);
        }

        public async Task<bool> IsEmailExistsAsync(string email)
        {
            return await _dbSet.AnyAsync(u => u.Email == email);
        }

        public async Task<bool> IsPhoneExistsAsync(string phone)
        {
            return await _dbSet.AnyAsync(u => u.Phone == phone);
        }

        public async Task UpdateRefreshTokenAsync(int userId, string refreshToken, DateTime expiredAt)
        {
            var user = await GetByIdAsync(userId);
            if (user != null)
            {
                user.RefreshToken = refreshToken;
                user.ExpiredRefreshTokenAt = expiredAt;
                await UpdateAsync(user);
            }
        }

        public async Task UpdatePasswordAsync(int userId, string newPassword)
        {
            var user = await GetByIdAsync(userId);
            if (user != null)
            {
                user.Password = newPassword;
                await UpdateAsync(user);
            }
        }

        public async Task UpdateUserStatusAsync(int userId, bool isActive, bool isVerify)
        {
            var user = await GetByIdAsync(userId);
            if (user != null)
            {
                user.IsActive = isActive;
                user.IsVerify = isVerify;
                await UpdateAsync(user);
            }
        }

        public async Task UpdateEmailVerificationTokenAsync(int userId, string verificationToken, DateTime expiredAt)
        {
            var user = await GetByIdAsync(userId);
            if (user != null)
            {
                user.RefreshToken = verificationToken; // Using RefreshToken field for verification token temporarily
                user.ExpiredRefreshTokenAt = expiredAt;
                await UpdateAsync(user);
            }
        }

        public async Task VerifyEmailAsync(int userId)
        {
            var user = await GetByIdAsync(userId);
            if (user != null)
            {
                user.IsVerify = true;
                user.RefreshToken = string.Empty; // Clear verification token
                user.ExpiredRefreshTokenAt = null;
                await UpdateAsync(user);
            }
        }

        public override async Task<User?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == id);
        }

        public override async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _dbSet
                .Include(u => u.Role)
                .ToListAsync();
        }
    }
}