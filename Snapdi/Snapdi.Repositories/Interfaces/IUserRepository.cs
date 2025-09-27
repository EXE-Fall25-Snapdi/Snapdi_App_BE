using Snapdi.Repositories.Models;

namespace Snapdi.Repositories.Interfaces
{
    public interface IUserRepository : IBaseRepository<User>
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByPhoneAsync(string phone);
        Task<User?> GetByEmailOrPhoneAsync(string emailOrPhone);
        Task<User?> GetByRefreshTokenAsync(string refreshToken);
        Task<User?> GetByEmailVerificationTokenAsync(string verificationToken);
        Task<IEnumerable<User>> GetUsersByRoleAsync(int roleId);
        Task<IEnumerable<User>> GetActiveUsersAsync();
        Task<IEnumerable<User>> GetVerifiedUsersAsync();
        Task<User?> GetUserWithRoleAsync(int userId);
        Task<User?> GetUserWithPhotographerProfileAsync(int userId);
        Task<bool> IsEmailExistsAsync(string email);
        Task<bool> IsPhoneExistsAsync(string phone);
        Task UpdateRefreshTokenAsync(int userId, string refreshToken, DateTime expiredAt);
        Task UpdatePasswordAsync(int userId, string newPassword);
        Task UpdateUserStatusAsync(int userId, bool isActive, bool isVerify);
        Task UpdateEmailVerificationTokenAsync(int userId, string verificationToken, DateTime expiredAt);
        Task VerifyEmailAsync(int userId);
    }
}