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
        Task<UserDto> CreateUserAsync(CreateUserDto createUserDto, bool isCreatedByAdmin = false);
        Task<UserDto?> UpdateUserAsync(int userId, UpdateUserDto updateUserDto);
        Task<bool> DeleteUserAsync(int userId);
        Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto);
        Task<bool> UpdateAvatarAsync(int userId, string avatarUrl);
        Task<bool> UpdateRefreshTokenAsync(int userId, string refreshToken, DateTime expiredAt);
        Task<bool> UpdateUserStatusAsync(int userId, bool isActive, bool isVerify);
        Task<bool> IsEmailExistsAsync(string email);
        Task<bool> IsPhoneExistsAsync(string phone);
        Task<UserDto?> AuthenticateAsync(string emailOrPhone, string password);
        Task<UserDto?> GetUserByRefreshTokenAsync(string refreshToken);
        
        // Token-based email verification methods (existing)
        Task<bool> SendEmailVerificationAsync(string email);
        Task<bool> VerifyEmailAsync(string verificationToken);
        Task<bool> ResendEmailVerificationAsync(string email);
        
        // Code-based email verification methods (new)
        Task<bool> SendVerificationCodeAsync(string email);
        Task<bool> VerifyEmailWithCodeAsync(string email, string code);
        Task<bool> ResendVerificationCodeAsync(string email);
        
        // User filtering method
        Task<PagedResultDto<UserDto>> GetUsersWithFilterAsync(UserFilterDto filterDto);
        
        // Photographer registration method
        Task<UserWithPhotographerDto> CreatePhotographerAsync(CreatePhotographerDto createPhotographerDto);
        
        // Photographer search method
        Task<PhotographerSearchResultDto> SearchPhotographersAsync(PhotographerSearchDto searchDto);
        
        // Find snappers (simplified photographer search)
        Task<FindSnapperResultDto> FindSnappersAsync(FindSnapperDto findSnapperDto);

        // Find snappers nearby for map display
        Task<FindSnappersNearbyResultDto> FindSnappersNearbyAsync(FindSnappersNearbyDto findSnappersNearbyDto);
        
        // Get photographers pending level assignment (for admin)
        Task<IEnumerable<UserWithPhotographerDto>> GetPhotographersPendingLevelAssignmentAsync();
        
        // Get photographers pending level assignment grouped by portfolio status (for admin)
        Task<PhotograhpersPendingLevelResponseDto> GetPhotographersPendingLevelAssignmentGroupedAsync();
        
        // Get photographers pending level assignment with paging and filtering (for admin)
        Task<PhotograhpersPendingLevelPagedResponseDto> GetPhotographersPendingLevelAssignmentPagedAsync(GetPhotographersPendingLevelRequestDto request);
        
        // Update photographer level (for admin)
        Task<bool> UpdatePhotographerLevelAsync(int userId, string levelPhotographer);
        
        // Update photographer availability status and location
        Task<bool> UpdatePhotographerStatusAsync(int userId, bool isAvailable, LocationCoordinatesDto? currentLocation = null);
        
        // Photo portfolio method
        Task<IEnumerable<PhotoPortfolioDto>> GetPhotoPortfoliosByUserIdAsync(int userId);
    }
}