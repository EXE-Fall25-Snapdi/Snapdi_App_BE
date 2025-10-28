using Snapdi.Repositories.Interfaces;
using Snapdi.Repositories.Models;
using Snapdi.Services.DTOs;
using Snapdi.Services.Interfaces;
using NetTopologySuite.Geometries;
using Snapdi.Repositories.Context;
using Microsoft.EntityFrameworkCore;

namespace Snapdi.Services.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IEmailService _emailService;
        private readonly IPhotographerProfileRepository _photographerProfileRepository;
        private readonly IVerificationCodeService _verificationCodeService;
        private readonly IPhotoPortfolioRepository _photoPortfolioRepository;
        private readonly IPhotographerPhotoTypeService _photographerPhotoTypeService;
        private readonly IPhotographerStyleRepository _photographerStyleRepository;

        public UserService(
            IUserRepository userRepository,
            IEmailService emailService,
            IPhotographerProfileRepository photographerProfileRepository,
            IVerificationCodeService verificationCodeService,
            IPhotoPortfolioRepository photoPortfolioRepository,
            IPhotographerPhotoTypeService photographerPhotoTypeService,
            IPhotographerStyleRepository photographerStyleRepository,
            SnapdiDbV2Context context)
        {
            _userRepository = userRepository;
            _emailService = emailService;
            _photographerProfileRepository = photographerProfileRepository;
            _verificationCodeService = verificationCodeService;
            _photoPortfolioRepository = photoPortfolioRepository;
            _photographerPhotoTypeService = photographerPhotoTypeService;
            _photographerStyleRepository = photographerStyleRepository;
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
                CreatedAt = DateTime.UtcNow,
                CurrentLocation = createUserDto.CurrentLocation != null
                    ? CreatePoint(createUserDto.CurrentLocation.Longitude, createUserDto.CurrentLocation.Latitude)
                    : null
            };

            var createdUser = await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();

            // Reload user with role information to get RoleName
            var userWithRole = await _userRepository.GetByIdAsync(createdUser.UserId);
            if (userWithRole == null)
            {
                userWithRole = createdUser;
            }

            // Send email verification only if not created by admin
            if (!isCreatedByAdmin)
            {
                // Use code-based verification by default
                await SendVerificationCodeAsync(userWithRole.Email);
            }
            else
            {
                // Send welcome email if created by admin (already verified)
                await _emailService.SendWelcomeEmailAsync(userWithRole.Email, userWithRole.Name);
            }

            return MapToUserDto(userWithRole);
        }

        public async Task<UserWithPhotographerDto> CreatePhotographerAsync(CreatePhotographerDto createPhotographerDto)
        {
            const int PHOTOGRAPHER_ROLE_ID = 3;

            // Hash password before saving
            var hashedPassword = HashPassword(createPhotographerDto.Password);

            var user = new User
            {
                Name = createPhotographerDto.Name,
                Email = createPhotographerDto.Email,
                Phone = createPhotographerDto.Phone ?? string.Empty,
                Password = hashedPassword,
                RoleId = PHOTOGRAPHER_ROLE_ID, // Set photographer role
                LocationAddress = createPhotographerDto.LocationAddress ?? string.Empty, // Now optional
                LocationCity = createPhotographerDto.LocationCity,
                AvatarUrl = createPhotographerDto.AvatarUrl ?? string.Empty,
                RefreshToken = string.Empty,
                IsActive = true,
                IsVerify = false, // Email verification required for public registration
                CreatedAt = DateTime.UtcNow,
                CurrentLocation = createPhotographerDto.CurrentLocation != null
                    ? CreatePoint(createPhotographerDto.CurrentLocation.Longitude, createPhotographerDto.CurrentLocation.Latitude)
                    : null
            };

            var createdUser = await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();

            // Create photographer profile
            var photographerProfile = new PhotographerProfile
            {
                UserId = createdUser.UserId,
                YearsOfExperience = createPhotographerDto.YearsOfExperience,
                EquipmentDescription = createPhotographerDto.EquipmentDescription,
                Description = createPhotographerDto.Description,
                LevelPhotographer = null, // Always null on registration - only admin can set this
                IsAvailable = createPhotographerDto.IsAvailable,
                AvgRating = 0.0, // Initial rating
                WorkLocation = createPhotographerDto.WorkLocation
            };

            await _photographerProfileRepository.AddAsync(photographerProfile);
            await _photographerProfileRepository.SaveChangesAsync();

            // Create PhotographerPhotoType associations with pricing using service
            if (createPhotographerDto.PhotographerPhotoTypes != null && createPhotographerDto.PhotographerPhotoTypes.Any())
            {
                await _photographerPhotoTypeService.AddMultipleAsync(createdUser.UserId, createPhotographerDto.PhotographerPhotoTypes);
            }

            // Create PhotographerStyle associations using repository
            if (createPhotographerDto.PhotographerStyleIds != null && createPhotographerDto.PhotographerStyleIds.Any())
            {
                await _photographerStyleRepository.AddMultiplePhotographerStylesAsync(createdUser.UserId, createPhotographerDto.PhotographerStyleIds);
                await _photographerStyleRepository.SaveChangesAsync();
            }

            // Send verification code
            await SendVerificationCodeAsync(createdUser.Email);

            // Reload user with complete information (without portfolios)
            var userWithPhotographer = await _userRepository.GetUserWithPhotographerProfileAsync(createdUser.UserId);

            return MapToUserWithPhotographerDtoWithoutPortfolio(userWithPhotographer!);
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

            if (updateUserDto.CurrentLocation != null)
                user.CurrentLocation = CreatePoint(updateUserDto.CurrentLocation.Longitude, updateUserDto.CurrentLocation.Latitude);

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

        // Token-based email verification methods (existing)
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

        // Code-based email verification methods (new)
        public async Task<bool> SendVerificationCodeAsync(string email)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null)
                return false;

            if (user.IsVerify)
                return false; // Already verified

            // Check rate limiting
            if (!_verificationCodeService.CanRequestNewCode(email))
                return false; // Too many requests

            // Generate verification code
            var verificationCode = _verificationCodeService.GenerateCode(email);

            // Send verification code email
            return await _emailService.SendVerificationCodeAsync(user.Email, user.Name, verificationCode);
        }

        public async Task<bool> VerifyEmailWithCodeAsync(string email, string code)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null)
                return false;

            if (user.IsVerify)
                return false; // Already verified

            // Verify the code
            if (!_verificationCodeService.VerifyCode(email, code))
                return false;

            // Mark user as verified
            await _userRepository.VerifyEmailAsync(user.UserId);
            await _userRepository.SaveChangesAsync();

            // Remove the used code
            _verificationCodeService.RemoveCode(email);

            // Send welcome email
            await _emailService.SendWelcomeEmailAsync(user.Email, user.Name);

            return true;
        }

        public async Task<bool> ResendVerificationCodeAsync(string email)
        {
            return await SendVerificationCodeAsync(email);
        }

        public async Task<IEnumerable<UserWithPhotographerDto>> GetPhotographersPendingLevelAssignmentAsync()
        {
            var photographers = await _userRepository.GetPhotographersPendingLevelAssignmentAsync();
            return photographers.Select(MapToUserWithPhotographerDto);
        }

        public async Task<PhotograhpersPendingLevelResponseDto> GetPhotographersPendingLevelAssignmentGroupedAsync()
        {
            var photographers = await _userRepository.GetPhotographersPendingLevelAssignmentAsync();
            var photographerDtos = photographers.Select(MapToUserWithPhotographerDto).ToList();

            var response = new PhotograhpersPendingLevelResponseDto();

            foreach (var photographer in photographerDtos)
            {
                // Check if photographer has any portfolio photos
                if (photographer.PhotoPortfolios != null && photographer.PhotoPortfolios.Any())
                {
                    response.WithPortfolio.Add(photographer);
                }
                else
                {
                    response.WithoutPortfolio.Add(photographer);
                }
            }

            // Sort by creation date (newest first) within each group
            response.WithPortfolio = response.WithPortfolio.OrderByDescending(p => p.CreatedAt).ToList();
            response.WithoutPortfolio = response.WithoutPortfolio.OrderByDescending(p => p.CreatedAt).ToList();

            return response;
        }

        public async Task<PhotograhpersPendingLevelPagedResponseDto> GetPhotographersPendingLevelAssignmentPagedAsync(GetPhotographersPendingLevelRequestDto request)
        {
            // Validate pagination parameters
            var page = Math.Max(1, request.Page);
            var pageSize = Math.Clamp(request.PageSize, 1, 50);

            var (withPortfolioUsers, withoutPortfolioUsers, withPortfolioTotalCount, withoutPortfolioTotalCount) =
                await _userRepository.GetPhotographersPendingLevelAssignmentPagedAsync(
                    page,
                    pageSize,
                    request.SearchTerm,
                    request.HasPortfolio,
                    request.LocationCity,
                    request.SortBy,
                    request.SortDirection,
                    request.CreatedFrom,
                    request.CreatedTo
                );

            // Map to DTOs
            var withPortfolioDtos = withPortfolioUsers.Select(MapToUserWithPhotographerDto).ToList();
            var withoutPortfolioDtos = withoutPortfolioUsers.Select(MapToUserWithPhotographerDto).ToList();

            // Calculate pagination info
            var withPortfolioTotalPages = (int)Math.Ceiling((double)withPortfolioTotalCount / pageSize);
            var withoutPortfolioTotalPages = (int)Math.Ceiling((double)withoutPortfolioTotalCount / pageSize);

            var response = new PhotograhpersPendingLevelPagedResponseDto
            {
                WithPortfolio = new PagedResultDto<UserWithPhotographerDto>
                {
                    Items = withPortfolioDtos,
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalItems = withPortfolioTotalCount,
                    TotalPages = withPortfolioTotalPages
                },
                WithoutPortfolio = new PagedResultDto<UserWithPhotographerDto>
                {
                    Items = withoutPortfolioDtos,
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalItems = withoutPortfolioTotalCount,
                    TotalPages = withoutPortfolioTotalPages
                },
                Summary = new PhotographersPendingLevelSummaryDto
                {
                    WithPortfolioCount = withPortfolioTotalCount,
                    WithoutPortfolioCount = withoutPortfolioTotalCount
                }
            };

            return response;
        }

        public async Task<bool> UpdatePhotographerLevelAsync(int userId, string levelPhotographer)
        {
            try
            {
                await _userRepository.UpdatePhotographerLevelAsync(userId, levelPhotographer);
                await _userRepository.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateAvatarAsync(int userId, string avatarUrl)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return false;
            await _userRepository.UpdateAvatarAsync(userId, avatarUrl);
            await _userRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdatePhotographerStatusAsync(int userId, bool isAvailable, LocationCoordinatesDto? currentLocation = null)
        {
            try
            {
                // Verify photographer profile exists
                var photographerProfile = await _photographerProfileRepository.GetByUserIdAsync(userId);
                if (photographerProfile == null)
                {
                    return false;
                }

                // Update photographer availability status
                await _photographerProfileRepository.UpdatePhotographerStatusAsync(userId, isAvailable);

                // Update current location if provided
                if (currentLocation != null)
                {
                    var user = await _userRepository.GetByIdAsync(userId);
                    if (user != null)
                    {
                        user.CurrentLocation = CreatePoint(currentLocation.Longitude, currentLocation.Latitude);
                        await _userRepository.UpdateAsync(user);
                    }
                }

                await _photographerProfileRepository.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<IEnumerable<PhotoPortfolioDto>> GetPhotoPortfoliosByUserIdAsync(int userId)
        {
            // Verify user exists
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return Enumerable.Empty<PhotoPortfolioDto>();
            }

            var portfolios = await _photoPortfolioRepository.GetByUserIdAsync(userId);
            return portfolios.Select(p => new PhotoPortfolioDto
            {
                PhotoPortfolioId = p.PhotoPortfolioId,
                UserId = p.UserId,
                PhotoUrl = p.PhotoUrl
            });
        }

        public async Task<PhotographerSearchResultDto> SearchPhotographersAsync(PhotographerSearchDto searchDto)
        {
            var (photographers, totalCount) = await _userRepository.SearchPhotographersAsync(
                searchDto.PageNumber,
                searchDto.PageSize,
                searchDto.SearchTerm,
                searchDto.LocationCity,
                searchDto.LevelPhotographer,
                searchDto.IsAvailable,
                searchDto.IsVerify,
                searchDto.IsActive,
                searchDto.MinRating,
                searchDto.MaxRating,
                searchDto.YearsOfExperience,
                searchDto.HasPortfolio,
                null, // styleIds - No style filtering in comprehensive search (for now)
                null, // workLocation - Not used in comprehensive search
                null, // photoTypeIds - Not used in comprehensive search
                null, // minPrice - Not used in comprehensive search
                null, // maxPrice - Not used in comprehensive search
                searchDto.CreatedFrom,
                searchDto.CreatedTo,
                searchDto.SortBy,
                searchDto.SortDirection
            );

            var photographerDtos = photographers.Select(MapToUserWithPhotographerDto).ToList();


            return new PhotographerSearchResultDto
            {
                Data = photographerDtos,
                TotalRecords = totalCount,
                PageNumber = searchDto.PageNumber,
                PageSize = searchDto.PageSize,
            };
        }

        public async Task<FindSnapperResultDto> FindSnappersAsync(FindSnapperDto findSnapperDto)
        {
            // Map FindSnapperDto to the repository's SearchPhotographersAsync parameters
            var (photographers, totalCount) = await _userRepository.SearchPhotographersAsync(
                findSnapperDto.Page,
                findSnapperDto.PageSize,
                searchTerm: null, // No search term in simplified search
                locationCity: null, // Deprecated - using workLocation instead
                levelPhotographer: null, // No level filtering
                isAvailable: findSnapperDto.IsAvailable,
                isVerify: true, // Only show verified photographers
                isActive: true, // Only show active photographers
                minRating: null,
                maxRating: null,
                yearsOfExperience: null,
                hasPortfolio: null,
                styleIds: findSnapperDto.StyleIds, // Pass style IDs for filtering
                workLocation: findSnapperDto.WorkLocation, // Use WorkLocation filter
                photoTypeIds: findSnapperDto.PhotoTypeIds, // Use PhotoTypeIds filter
                minPrice: findSnapperDto.MinPrice, // Use MinPrice filter
                maxPrice: findSnapperDto.MaxPrice, // Use MaxPrice filter
                createdFrom: null,
                createdTo: null,
                sortBy: findSnapperDto.SortBy,
                sortDirection: findSnapperDto.SortDirection
            );

            // Map to SnapperDto
            var snapperDtos = photographers.Select(MapToSnapperDto).ToList();

            // Count available snappers in results
            var availableCount = snapperDtos.Count(s => s.IsAvailable);

            return new FindSnapperResultDto
            {
                Snappers = snapperDtos,
                TotalCount = totalCount,
                CurrentPage = findSnapperDto.Page,
                PageSize = findSnapperDto.PageSize,
                AvailableCount = availableCount
            };
        }

        public async Task<FindSnappersNearbyResultDto> FindSnappersNearbyAsync(FindSnappersNearbyDto findSnappersNearbyDto)
        {
            // Call repository method with geographic filtering
            var photographersWithDistance = await _userRepository.FindSnappersNearbyAsync(
                findSnappersNearbyDto.Latitude,
                findSnappersNearbyDto.Longitude,
                findSnappersNearbyDto.RadiusInKm,
                findSnappersNearbyDto.Limit,
                findSnappersNearbyDto.IsAvailable,
                findSnappersNearbyDto.PhotoTypeIds,
                findSnappersNearbyDto.StyleIds,
                findSnappersNearbyDto.MinPrice,
                findSnappersNearbyDto.MaxPrice
            );

            // Map to SnapperMapDto
            var snapperMapDtos = photographersWithDistance.Select(pd => MapToSnapperMapDto(pd.User, pd.DistanceInKm)).ToList();

            // Count available snappers
            var availableCount = snapperMapDtos.Count(s => s.IsAvailable);

            return new FindSnappersNearbyResultDto
            {
                Snappers = snapperMapDtos,
                TotalCount = snapperMapDtos.Count,
                SearchCenter = new LocationCoordinatesDto
                {
                    Latitude = findSnappersNearbyDto.Latitude,
                    Longitude = findSnappersNearbyDto.Longitude
                },
                RadiusInKm = findSnappersNearbyDto.RadiusInKm,
                AvailableCount = availableCount
            };
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
                AvatarUrl = string.IsNullOrEmpty(user.AvatarUrl) ? null : user.AvatarUrl,
                CurrentLocation = user.CurrentLocation != null
                    ? new LocationCoordinatesDto
                    {
                        Latitude = user.CurrentLocation.Y,
                        Longitude = user.CurrentLocation.X
                    }
                    : null
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
                AvatarUrl = string.IsNullOrEmpty(user.AvatarUrl) ? null : user.AvatarUrl,
                CurrentLocation = user.CurrentLocation != null
                    ? new LocationCoordinatesDto
                    {
                        Latitude = user.CurrentLocation.Y,
                        Longitude = user.CurrentLocation.X
                    }
                    : null
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
                    Description = user.PhotographerProfile.Description,
                    LevelPhotographer = user.PhotographerProfile.LevelPhotographer,
                    WorkLocation = user.PhotographerProfile.WorkLocation
                };

                // Map photographer styles
                if (user.PhotographerProfile.PhotographerStyles?.Any() == true)
                {
                    userDto.PhotographerProfile.PhotographerStyles = user.PhotographerProfile.PhotographerStyles
                  .Where(ps => ps.Style != null)
                              .Select(ps => new StyleDto
                              {
                                  StyleId = ps.StyleId,
                                  StyleName = ps.Style.StyleName
                              })
                          .ToList();
                }

                // Map photo types with price and time from PhotographerPhotoType
                if (user.PhotographerProfile.PhotographerPhotoTypes?.Any() == true)
                {
                    userDto.PhotographerProfile.PhotoTypes = user.PhotographerProfile.PhotographerPhotoTypes
                  .Where(ppt => ppt.PhotoType != null)
                .Select(ppt => new PhotoTypeWithPricingResponseDto
                {
                    PhotoTypeId = ppt.PhotoTypeId,
                    PhotoTypeName = ppt.PhotoType?.PhotoTypeName,
                    PhotoPrice = ppt.PhotoPrice,
                    Time = ppt.Time
                })
            .ToList();
                }

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

        private static UserWithPhotographerDto MapToUserWithPhotographerDtoWithoutPortfolio(User user)
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
                AvatarUrl = string.IsNullOrEmpty(user.AvatarUrl) ? null : user.AvatarUrl,
                CurrentLocation = user.CurrentLocation != null
                    ? new LocationCoordinatesDto
                    {
                        Latitude = user.CurrentLocation.Y,
                        Longitude = user.CurrentLocation.X
                    }
                    : null
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
                    Description = user.PhotographerProfile.Description,
                    LevelPhotographer = user.PhotographerProfile.LevelPhotographer,
                    WorkLocation = user.PhotographerProfile.WorkLocation
                };

                // Map photographer styles
                if (user.PhotographerProfile.PhotographerStyles?.Any() == true)
                {
                    userDto.PhotographerProfile.PhotographerStyles = user.PhotographerProfile.PhotographerStyles
                  .Where(ps => ps.Style != null)
                              .Select(ps => new StyleDto
                              {
                                  StyleId = ps.StyleId,
                                  StyleName = ps.Style.StyleName
                              })
                          .ToList();
                }

                // Map photo types with price and time from PhotographerPhotoType
                if (user.PhotographerProfile.PhotographerPhotoTypes?.Any() == true)
                {
                    userDto.PhotographerProfile.PhotoTypes = user.PhotographerProfile.PhotographerPhotoTypes
                  .Where(ppt => ppt.PhotoType != null)
                .Select(ppt => new PhotoTypeWithPricingResponseDto
                {
                    PhotoTypeId = ppt.PhotoTypeId,
                    PhotoTypeName = ppt.PhotoType?.PhotoTypeName,
                    PhotoPrice = ppt.PhotoPrice,
                    Time = ppt.Time
                })
            .ToList();
                }

            }

            // Explicitly set PhotoPortfolios to null to exclude from response
            userDto.PhotoPortfolios = null;

            return userDto;
        }

        private static SnapperDto MapToSnapperDto(User user)
        {
            var snapperDto = new SnapperDto
            {
                UserId = user.UserId,
                Name = user.Name,
                Email = user.Email,
                Phone = string.IsNullOrEmpty(user.Phone) ? null : user.Phone,
                LocationCity = string.IsNullOrEmpty(user.LocationCity) ? null : user.LocationCity,
                LocationAddress = string.IsNullOrEmpty(user.LocationAddress) ? null : user.LocationAddress,
                AvatarUrl = string.IsNullOrEmpty(user.AvatarUrl) ? null : user.AvatarUrl,
                IsActive = user.IsActive,
                IsVerify = user.IsVerify,
                CurrentLocation = user.CurrentLocation != null
                    ? new LocationCoordinatesDto
                    {
                        Latitude = user.CurrentLocation.Y,
                        Longitude = user.CurrentLocation.X
                    }
                    : null,
                PortfolioCount = 0,
                PortfolioUrls = new List<string>()
            };

            // Map photographer profile
            if (user.PhotographerProfile != null)
            {
                snapperDto.LevelPhotographer = user.PhotographerProfile.LevelPhotographer;
                snapperDto.IsAvailable = user.PhotographerProfile.IsAvailable;
                snapperDto.AvgRating = user.PhotographerProfile.AvgRating;
                snapperDto.YearsOfExperience = user.PhotographerProfile.YearsOfExperience;
                snapperDto.EquipmentDescription = user.PhotographerProfile.EquipmentDescription;
                snapperDto.Description = user.PhotographerProfile.Description;
                snapperDto.WorkLocation = user.PhotographerProfile.WorkLocation;

                // Map styles
                if (user.PhotographerProfile.PhotographerStyles?.Any() == true)
                {
                    snapperDto.Styles = user.PhotographerProfile.PhotographerStyles
                            .Where(ps => ps.Style != null)
                   .Select(ps => new StyleDto
                   {
                       StyleId = ps.StyleId,
                       StyleName = ps.Style.StyleName
                   })
                    .ToList();
                }

                // Map photo types with price and time from PhotographerPhotoType
                if (user.PhotographerProfile.PhotographerPhotoTypes?.Any() == true)
                {
                    snapperDto.PhotoTypes = user.PhotographerProfile.PhotographerPhotoTypes
               .Where(ppt => ppt.PhotoType != null)
                 .Select(ppt => new PhotoTypeWithPricingResponseDto
                 {
                     PhotoTypeId = ppt.PhotoTypeId,
                     PhotoTypeName = ppt.PhotoType?.PhotoTypeName,
                     PhotoPrice = ppt.PhotoPrice,
                     Time = ppt.Time
                 })
                     .ToList();
                }
            }

            // Map photo portfolios
            if (user.PhotoPortfolios?.Any() == true)
            {
                snapperDto.PortfolioCount = user.PhotoPortfolios.Count;
                snapperDto.PortfolioUrls = user.PhotoPortfolios.Select(p => p.PhotoUrl).ToList();
            }

            return snapperDto;
        }

        private static SnapperMapDto MapToSnapperMapDto(User user, double distanceInKm)
        {
            var snapperMapDto = new SnapperMapDto
            {
                UserId = user.UserId,
                Name = user.Name,
                AvatarUrl = string.IsNullOrEmpty(user.AvatarUrl) ? null : user.AvatarUrl,
                CurrentLocation = new LocationCoordinatesDto
                {
                    Latitude = user.CurrentLocation!.Y,
                    Longitude = user.CurrentLocation.X
                },
                DistanceInKm = distanceInKm,
                LocationCity = string.IsNullOrEmpty(user.LocationCity) ? null : user.LocationCity,
                IsAvailable = false
            };

            // Map photographer profile
            if (user.PhotographerProfile != null)
            {
                snapperMapDto.LevelPhotographer = user.PhotographerProfile.LevelPhotographer;
                snapperMapDto.IsAvailable = user.PhotographerProfile.IsAvailable;
                snapperMapDto.AvgRating = user.PhotographerProfile.AvgRating;

                // Truncate description for map display (max 100 characters)
                if (!string.IsNullOrEmpty(user.PhotographerProfile.Description))
                {
                    snapperMapDto.Description = user.PhotographerProfile.Description.Length > 100
                        ? user.PhotographerProfile.Description.Substring(0, 97) + "..."
                        : user.PhotographerProfile.Description;
                }

                // Map photo types from PhotographerPhotoType with pricing
                if (user.PhotographerProfile.PhotographerPhotoTypes?.Any() == true)
                {
                    snapperMapDto.PhotoTypes = user.PhotographerProfile.PhotographerPhotoTypes
                        .Where(ppt => ppt.PhotoType != null)
                        .Select(ppt => new PhotoTypeWithPriceDto
                        {
                            PhotoTypeName = ppt.PhotoType.PhotoTypeName,
                            PhotoPrice = ppt.PhotoPrice,
                            Time = ppt.Time
                        })
                        .ToList();
                }

                // Map styles as simple list of names
                if (user.PhotographerProfile.PhotographerStyles?.Any() == true)
                {
                    snapperMapDto.Styles = user.PhotographerProfile.PhotographerStyles
                        .Where(ps => ps.Style != null)
                        .Select(ps => ps.Style.StyleName)
                        .ToList();
                }
            }

            return snapperMapDto;
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

        /// <summary>
        /// Creates a Point geometry for geographic coordinates.
        /// Uses SRID 4326 (WGS84) which is standard for GPS coordinates.
        /// Note: Point constructor takes (longitude, latitude) - longitude comes first!
        /// </summary>
        private static Point CreatePoint(double longitude, double latitude)
        {
            var geometryFactory = NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
            return geometryFactory.CreatePoint(new Coordinate(longitude, latitude));
        }

        #endregion
    }
}