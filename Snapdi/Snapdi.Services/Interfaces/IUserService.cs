using Snapdi.Services.DTOs;

namespace Snapdi.Services.Interfaces
{
    public interface IUserService
    {
        Task<UserDto?> GetUserByIdAsync(int userId);
        Task<UserDto?> GetUserByEmailAsync(string email);
        Task<UserDto?> GetUserByPhoneAsync(string phone);
        Task<IEnumerable<UserDto>> GetAllUsersAsync();
        Task<IEnumerable<UserDto>> GetUsersByRoleAsync(int roleId);
        Task<IEnumerable<UserDto>> GetActiveUsersAsync();
        Task<IEnumerable<UserDto>> GetVerifiedUsersAsync();
        Task<UserWithPhotographerDto?> GetUserWithPhotographerProfileAsync(int userId);
        Task<UserDto> CreateUserAsync(CreateUserDto createUserDto);
        Task<UserDto?> UpdateUserAsync(int userId, UpdateUserDto updateUserDto);
        Task<bool> DeleteUserAsync(int userId);
        Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto);
        Task<bool> UpdateRefreshTokenAsync(int userId, string refreshToken, DateTime expiredAt);
        Task<bool> UpdateUserStatusAsync(int userId, bool isActive, bool isVerify);
        Task<bool> IsEmailExistsAsync(string email);
        Task<bool> IsPhoneExistsAsync(string phone);
        Task<UserDto?> AuthenticateAsync(string emailOrPhone, string password);
        Task<UserDto?> GetUserByRefreshTokenAsync(string refreshToken);
        
        // Email verification methods
        Task<bool> SendEmailVerificationAsync(string email);
        Task<bool> VerifyEmailAsync(string verificationToken);
        Task<bool> ResendEmailVerificationAsync(string email);
    }
}