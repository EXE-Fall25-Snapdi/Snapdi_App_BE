using Snapdi.Repositories.Interfaces;
using Snapdi.Repositories.Models;
using Snapdi.Services.DTOs;
using Snapdi.Services.Interfaces;

namespace Snapdi.Services.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IEmailService _emailService;

        public UserService(IUserRepository userRepository, IEmailService emailService)
        {
            _userRepository = userRepository;
            _emailService = emailService;
        }

        public async Task<UserDto?> GetUserByIdAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            return user != null ? MapToUserDto(user) : null;
        }

        public async Task<UserDto?> GetUserByEmailAsync(string email)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            return user != null ? MapToUserDto(user) : null;
        }

        public async Task<UserDto?> GetUserByPhoneAsync(string phone)
        {
            var user = await _userRepository.GetByPhoneAsync(phone);
            return user != null ? MapToUserDto(user) : null;
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllAsync();
            return users.Select(MapToUserDto);
        }

        public async Task<IEnumerable<UserDto>> GetUsersByRoleAsync(int roleId)
        {
            var users = await _userRepository.GetUsersByRoleAsync(roleId);
            return users.Select(MapToUserDto);
        }

        public async Task<IEnumerable<UserDto>> GetActiveUsersAsync()
        {
            var users = await _userRepository.GetActiveUsersAsync();
            return users.Select(MapToUserDto);
        }

        public async Task<IEnumerable<UserDto>> GetVerifiedUsersAsync()
        {
            var users = await _userRepository.GetVerifiedUsersAsync();
            return users.Select(MapToUserDto);
        }

        public async Task<UserWithPhotographerDto?> GetUserWithPhotographerProfileAsync(int userId)
        {
            var user = await _userRepository.GetUserWithPhotographerProfileAsync(userId);
            return user != null ? MapToUserWithPhotographerDto(user) : null;
        }

        public async Task<UserDto> CreateUserAsync(CreateUserDto createUserDto)
        {
            return await CreateUserAsync(createUserDto, false);
        }

        public async Task<UserDto> CreateUserAsync(CreateUserDto createUserDto, bool isCreatedByAdmin = false)
        {
            // Hash password before saving
            var hashedPassword = HashPassword(createUserDto.Password);

            var user = new User
            {
                Name = createUserDto.Name,
                Email = createUserDto.Email,
                Phone = createUserDto.Phone ?? string.Empty,
                Password = hashedPassword,
                RoleId = createUserDto.RoleId,
                LocationAddress = createUserDto.LocationAddress ?? string.Empty,
                LocationCity = createUserDto.LocationCity ?? string.Empty,
                AvatarUrl = createUserDto.AvatarUrl ?? string.Empty,
                RefreshToken = string.Empty,
                IsActive = true,
                IsVerify = isCreatedByAdmin, // Auto-verify if created by admin
                CreatedAt = DateTime.UtcNow
            };

            var createdUser = await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();

            // Reload user with role information to get RoleName
            var userWithRole = await _userRepository.GetByIdAsync(createdUser.UserId);
            if (userWithRole == null)
            {
                // Fallback: if we can't reload the user with role, use the created user
                userWithRole = createdUser;
            }

            // Send email verification only if not created by admin
            if (!isCreatedByAdmin)
            {
                await SendEmailVerificationAsync(userWithRole.Email);
            }
            else
            {
                // Send welcome email if created by admin (already verified)
                await _emailService.SendWelcomeEmailAsync(userWithRole.Email, userWithRole.Name);
            }

            return MapToUserDto(userWithRole);
        }

        public async Task<PagedResultDto<UserDto>> GetUsersWithFilterAsync(UserFilterDto filterDto)
        {
            // Handle null sortDirection by providing default
            var sortDirection = string.IsNullOrEmpty(filterDto.SortDirection) ? "asc" : filterDto.SortDirection;
            
            var (users, totalCount) = await _userRepository.GetUsersWithFilterAsync(
                filterDto.Page,
                filterDto.PageSize,
                filterDto.SearchTerm,
                filterDto.RoleId,
                filterDto.IsActive,
                filterDto.IsVerified,
                filterDto.LocationCity,
                filterDto.SortBy,
                sortDirection,
                filterDto.CreatedFrom,
                filterDto.CreatedTo
            );

            var userDtos = users.Select(MapToUserDto).ToList();
            var totalPages = (int)Math.Ceiling((double)totalCount / filterDto.PageSize);

            return new PagedResultDto<UserDto>
            {
                Items = userDtos,
                CurrentPage = filterDto.Page,
                PageSize = filterDto.PageSize,
                TotalItems = totalCount,
                TotalPages = totalPages
            };
        }

        public async Task<UserDto?> UpdateUserAsync(int userId, UpdateUserDto updateUserDto)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return null;

            if (!string.IsNullOrEmpty(updateUserDto.Name))
                user.Name = updateUserDto.Name;

            if (updateUserDto.Phone != null)
                user.Phone = updateUserDto.Phone;

            if (updateUserDto.LocationAddress != null)
                user.LocationAddress = updateUserDto.LocationAddress;

            if (updateUserDto.LocationCity != null)
                user.LocationCity = updateUserDto.LocationCity;

            if (updateUserDto.AvatarUrl != null)
                user.AvatarUrl = updateUserDto.AvatarUrl;

            if (updateUserDto.IsActive.HasValue)
                user.IsActive = updateUserDto.IsActive.Value;

            if (updateUserDto.IsVerify.HasValue)
                user.IsVerify = updateUserDto.IsVerify.Value;

            await _userRepository.UpdateAsync(user);
            await _userRepository.SaveChangesAsync();

            return MapToUserDto(user);
        }

        public async Task<bool> DeleteUserAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return false;

            await _userRepository.DeleteAsync(user);
            await _userRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return false;

            // Verify current password
            if (!VerifyPassword(changePasswordDto.CurrentPassword, user.Password))
                return false;

            // Hash new password
            var hashedNewPassword = HashPassword(changePasswordDto.NewPassword);
            await _userRepository.UpdatePasswordAsync(userId, hashedNewPassword);
            await _userRepository.SaveChangesAsync();

            return true;
        }

        public async Task<bool> UpdateRefreshTokenAsync(int userId, string refreshToken, DateTime expiredAt)
        {
            await _userRepository.UpdateRefreshTokenAsync(userId, refreshToken, expiredAt);
            await _userRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateUserStatusAsync(int userId, bool isActive, bool isVerify)
        {
            await _userRepository.UpdateUserStatusAsync(userId, isActive, isVerify);
            await _userRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsEmailExistsAsync(string email)
        {
            return await _userRepository.IsEmailExistsAsync(email);
        }

        public async Task<bool> IsPhoneExistsAsync(string phone)
        {
            return await _userRepository.IsPhoneExistsAsync(phone);
        }

        public async Task<UserDto?> AuthenticateAsync(string emailOrPhone, string password)
        {
            // Try to find user by email or phone
            var user = await _userRepository.GetByEmailOrPhoneAsync(emailOrPhone);

            if (user == null || !VerifyPassword(password, user.Password))
                return null;

            return MapToUserDto(user);
        }

        public async Task<UserDto?> GetUserByRefreshTokenAsync(string refreshToken)
        {
            var user = await _userRepository.GetByRefreshTokenAsync(refreshToken);
            return user != null ? MapToUserDto(user) : null;
        }

        public async Task<bool> SendEmailVerificationAsync(string email)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null)
                return false;

            if (user.IsVerify)
                return false; // Already verified

            // Generate verification token
            var verificationToken = GenerateVerificationToken();
            var expiryTime = DateTime.UtcNow.AddHours(24); // Token expires in 24 hours

            // Save verification token
            await _userRepository.UpdateEmailVerificationTokenAsync(user.UserId, verificationToken, expiryTime);
            await _userRepository.SaveChangesAsync();

            // Send verification email
            return await _emailService.SendEmailVerificationAsync(user.Email, user.Name, verificationToken);
        }

        public async Task<bool> VerifyEmailAsync(string verificationToken)
        {
            var user = await _userRepository.GetByEmailVerificationTokenAsync(verificationToken);
            if (user == null)
                return false;

            if (user.IsVerify)
                return false; // Already verified

            // Verify the user
            await _userRepository.VerifyEmailAsync(user.UserId);
            await _userRepository.SaveChangesAsync();

            // Send welcome email
            await _emailService.SendWelcomeEmailAsync(user.Email, user.Name);

            return true;
        }

        public async Task<bool> ResendEmailVerificationAsync(string email)
        {
            return await SendEmailVerificationAsync(email);
        }

        #region Private Methods

        private static UserDto MapToUserDto(User user)
        {
            return new UserDto
            {
                UserId = user.UserId,
                RoleId = user.RoleId,
                RoleName = user.Role?.RoleName,
                Name = user.Name,
                Email = user.Email,
                Phone = string.IsNullOrEmpty(user.Phone) ? null : user.Phone,
                IsActive = user.IsActive,
                IsVerify = user.IsVerify,
                CreatedAt = user.CreatedAt,
                LocationAddress = string.IsNullOrEmpty(user.LocationAddress) ? null : user.LocationAddress,
                LocationCity = string.IsNullOrEmpty(user.LocationCity) ? null : user.LocationCity,
                AvatarUrl = string.IsNullOrEmpty(user.AvatarUrl) ? null : user.AvatarUrl
            };
        }

        private static UserWithPhotographerDto MapToUserWithPhotographerDto(User user)
        {
            var userDto = new UserWithPhotographerDto
            {
                UserId = user.UserId,
                RoleId = user.RoleId,
                RoleName = user.Role?.RoleName,
                Name = user.Name,
                Email = user.Email,
                Phone = string.IsNullOrEmpty(user.Phone) ? null : user.Phone,
                IsActive = user.IsActive,
                IsVerify = user.IsVerify,
                CreatedAt = user.CreatedAt,
                LocationAddress = string.IsNullOrEmpty(user.LocationAddress) ? null : user.LocationAddress,
                LocationCity = string.IsNullOrEmpty(user.LocationCity) ? null : user.LocationCity,
                AvatarUrl = string.IsNullOrEmpty(user.AvatarUrl) ? null : user.AvatarUrl
            };

            // Map photographer profile
            if (user.PhotographerProfile != null)
            {
                userDto.PhotographerProfile = new PhotographerProfileDto
                {
                    UserId = user.PhotographerProfile.UserId,
                    EquipmentDescription = user.PhotographerProfile.EquipmentDescription,
                    YearsOfExperience = user.PhotographerProfile.YearsOfExperience,
                    AvgRating = user.PhotographerProfile.AvgRating,
                    IsAvailable = user.PhotographerProfile.IsAvailable,
                    Description = user.PhotographerProfile.Description
                };
            }

            // Map photo portfolios
            if (user.PhotoPortfolios?.Any() == true)
            {
                userDto.PhotoPortfolios = user.PhotoPortfolios.Select(p => new PhotoPortfolioDto
                {
                    PhotoPortfolioId = p.PhotoPortfolioId,
                    UserId = p.UserId,
                    PhotoUrl = p.PhotoUrl
                }).ToList();
            }

            return userDto;
        }

        private static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        private static bool VerifyPassword(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }

        private static string GenerateVerificationToken()
        {
            return Guid.NewGuid().ToString("N") + DateTime.UtcNow.Ticks.ToString();
        }

        #endregion
    }
}