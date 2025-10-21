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
        public async Task<User?> GetAdminUserAsync()
        {
            return await _context.Users
                .Include(u => u.Role)
                .Where(u => u.Role.RoleName == "ADMIN")
                .FirstOrDefaultAsync();
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

        public async Task<IEnumerable<User>> GetPhotographersPendingLevelAssignmentAsync()
        {
            const int PHOTOGRAPHER_ROLE_ID = 3;
            
            return await _context.Users
                .Include(u => u.Role)
                .Include(u => u.PhotographerProfile)
                .Where(u => u.RoleId == PHOTOGRAPHER_ROLE_ID && 
                           u.IsVerify == true && 
                           u.PhotographerProfile != null && 
                           u.PhotographerProfile.IsAvailable == false && 
                           (u.PhotographerProfile.LevelPhotographer == null || u.PhotographerProfile.LevelPhotographer == ""))
                .ToListAsync();
        }

        public async Task<(IEnumerable<User> WithPortfolio, IEnumerable<User> WithoutPortfolio, int WithPortfolioTotalCount, int WithoutPortfolioTotalCount)> 
            GetPhotographersPendingLevelAssignmentPagedAsync(
                int page,
                int pageSize,
                string? searchTerm = null,
                bool? hasPortfolio = null,
                string? locationCity = null,
                string? sortBy = "createdAt",
                string? sortDirection = "desc",
                DateTime? createdFrom = null,
                DateTime? createdTo = null)
        {
            const int PHOTOGRAPHER_ROLE_ID = 3;
            
            // Base query for photographers pending level assignment
            var baseQuery = _context.Users
                .Include(u => u.Role)
                .Include(u => u.PhotographerProfile)
                .Include(u => u.PhotoPortfolios)
                .Where(u => u.RoleId == PHOTOGRAPHER_ROLE_ID && 
                           u.IsVerify == true && 
                           u.PhotographerProfile != null && 
                           u.PhotographerProfile.IsAvailable == false && 
                           (u.PhotographerProfile.LevelPhotographer == null || u.PhotographerProfile.LevelPhotographer == ""));

            // Apply common filters
            if (!string.IsNullOrEmpty(searchTerm))
            {
                var searchLower = searchTerm.ToLower();
                baseQuery = baseQuery.Where(u => 
                    u.Name.ToLower().Contains(searchLower) || 
                    u.Email.ToLower().Contains(searchLower));
            }

            if (!string.IsNullOrEmpty(locationCity))
            {
                baseQuery = baseQuery.Where(u => u.LocationCity != null && u.LocationCity.ToLower().Contains(locationCity.ToLower()));
            }

            if (createdFrom.HasValue)
            {
                baseQuery = baseQuery.Where(u => u.CreatedAt >= createdFrom.Value);
            }

            if (createdTo.HasValue)
            {
                baseQuery = baseQuery.Where(u => u.CreatedAt <= createdTo.Value);
            }

            // Split into with/without portfolio groups
            var withPortfolioQuery = baseQuery.Where(u => u.PhotoPortfolios.Any());
            var withoutPortfolioQuery = baseQuery.Where(u => !u.PhotoPortfolios.Any());

            // Apply portfolio filter if specified
            IQueryable<User> finalQuery;
            if (hasPortfolio == true)
            {
                finalQuery = withPortfolioQuery;
                withoutPortfolioQuery = _context.Users.Where(u => false); // Empty query
            }
            else if (hasPortfolio == false)
            {
                finalQuery = withoutPortfolioQuery;
                withPortfolioQuery = _context.Users.Where(u => false); // Empty query
            }
            else
            {
                finalQuery = baseQuery;
            }

            // Apply sorting to both queries
            var isDescending = sortDirection?.ToLower() == "desc";
            
            if (!string.IsNullOrEmpty(sortBy))
            {
                withPortfolioQuery = sortBy.ToLower() switch
                {
                    "name" => isDescending ? withPortfolioQuery.OrderByDescending(u => u.Name) : withPortfolioQuery.OrderBy(u => u.Name),
                    "email" => isDescending ? withPortfolioQuery.OrderByDescending(u => u.Email) : withPortfolioQuery.OrderBy(u => u.Email),
                    "createdat" => isDescending ? withPortfolioQuery.OrderByDescending(u => u.CreatedAt) : withPortfolioQuery.OrderBy(u => u.CreatedAt),
                    _ => isDescending ? withPortfolioQuery.OrderByDescending(u => u.CreatedAt) : withPortfolioQuery.OrderBy(u => u.CreatedAt)
                };

                withoutPortfolioQuery = sortBy.ToLower() switch
                {
                    "name" => isDescending ? withoutPortfolioQuery.OrderByDescending(u => u.Name) : withoutPortfolioQuery.OrderBy(u => u.Name),
                    "email" => isDescending ? withoutPortfolioQuery.OrderByDescending(u => u.Email) : withoutPortfolioQuery.OrderBy(u => u.Email),
                    "createdat" => isDescending ? withoutPortfolioQuery.OrderByDescending(u => u.CreatedAt) : withoutPortfolioQuery.OrderBy(u => u.CreatedAt),
                    _ => isDescending ? withoutPortfolioQuery.OrderByDescending(u => u.CreatedAt) : withoutPortfolioQuery.OrderBy(u => u.CreatedAt)
                };
            }
            else
            {
                withPortfolioQuery = isDescending ? withPortfolioQuery.OrderByDescending(u => u.CreatedAt) : withPortfolioQuery.OrderBy(u => u.CreatedAt);
                withoutPortfolioQuery = isDescending ? withoutPortfolioQuery.OrderByDescending(u => u.CreatedAt) : withoutPortfolioQuery.OrderBy(u => u.CreatedAt);
            }

            // Get total counts
            var withPortfolioTotalCount = await withPortfolioQuery.CountAsync();
            var withoutPortfolioTotalCount = await withoutPortfolioQuery.CountAsync();

            // Apply pagination
            var withPortfolioResults = await withPortfolioQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var withoutPortfolioResults = await withoutPortfolioQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (withPortfolioResults, withoutPortfolioResults, withPortfolioTotalCount, withoutPortfolioTotalCount);
        }

        public async Task UpdatePhotographerLevelAsync(int userId, string levelPhotographer)
        {
            var photographerProfile = await _context.PhotographerProfiles.FindAsync(userId);
            if (photographerProfile != null)
            {
                photographerProfile.LevelPhotographer = levelPhotographer;
            }
        }

        public async Task<(IEnumerable<User> Photographers, int TotalCount)> SearchPhotographersAsync(
            int page,
            int pageSize,
            string? searchTerm = null,
            string? locationCity = null,
            string? levelPhotographer = null,
            bool? isAvailable = null,
            bool? isVerify = null,
            bool? isActive = null,
            double? minRating = null,
            double? maxRating = null,
            string? yearsOfExperience = null,
            bool? hasPortfolio = null,
            List<int>? styleIds = null,
            string? workLocation = null,
            DateTime? createdFrom = null,
            DateTime? createdTo = null,
            string? sortBy = "createdAt",
            string? sortDirection = "desc")
        {
            const int PHOTOGRAPHER_ROLE_ID = 3;
            
            var query = _context.Users
                .Include(u => u.Role)
                .Include(u => u.PhotographerProfile)
                    .ThenInclude(pp => pp.PhotographerStyles)
                        .ThenInclude(ps => ps.Style)
                .Include(u => u.PhotoPortfolios)
                .Where(u => u.RoleId == PHOTOGRAPHER_ROLE_ID && u.PhotographerProfile != null)
                .AsQueryable();

            // Apply search term filter
            if (!string.IsNullOrEmpty(searchTerm))
            {
                var searchLower = searchTerm.ToLower();
                query = query.Where(u => 
                    u.Name.ToLower().Contains(searchLower) || 
                    u.Email.ToLower().Contains(searchLower) ||
                    (u.PhotographerProfile!.Description != null && u.PhotographerProfile.Description.ToLower().Contains(searchLower)));
            }

            // Apply location filter (for backward compatibility)
            if (!string.IsNullOrEmpty(locationCity))
            {
                query = query.Where(u => u.LocationCity != null && u.LocationCity.ToLower().Contains(locationCity.ToLower()));
            }

            // Apply work location filter (searches in PhotographerProfile.WorkLocation)
            if (!string.IsNullOrEmpty(workLocation))
            {
                query = query.Where(u => u.PhotographerProfile!.WorkLocation != null && 
                                        u.PhotographerProfile.WorkLocation.ToLower().Contains(workLocation.ToLower()));
            }

            // Apply photographer level filter
            if (!string.IsNullOrEmpty(levelPhotographer))
            {
                query = query.Where(u => u.PhotographerProfile!.LevelPhotographer != null && 
                                        u.PhotographerProfile.LevelPhotographer.ToLower() == levelPhotographer.ToLower());
            }

            // Apply availability filter
            if (isAvailable.HasValue)
            {
                query = query.Where(u => u.PhotographerProfile!.IsAvailable == isAvailable.Value);
            }

            // Apply verification filter
            if (isVerify.HasValue)
            {
                query = query.Where(u => u.IsVerify == isVerify.Value);
            }

            // Apply active status filter
            if (isActive.HasValue)
            {
                query = query.Where(u => u.IsActive == isActive.Value);
            }

            // Apply rating filters
            if (minRating.HasValue)
            {
                query = query.Where(u => u.PhotographerProfile!.AvgRating >= minRating.Value);
            }

            if (maxRating.HasValue)
            {
                query = query.Where(u => u.PhotographerProfile!.AvgRating <= maxRating.Value);
            }

            // Apply years of experience filter
            if (!string.IsNullOrEmpty(yearsOfExperience))
            {
                query = query.Where(u => u.PhotographerProfile!.YearsOfExperience != null && 
                                        u.PhotographerProfile.YearsOfExperience.ToLower().Contains(yearsOfExperience.ToLower()));
            }

            // Apply portfolio filter
            if (hasPortfolio.HasValue)
            {
                if (hasPortfolio.Value)
                {
                    query = query.Where(u => u.PhotoPortfolios.Any());
                }
                else
                {
                    query = query.Where(u => !u.PhotoPortfolios.Any());
                }
            }

            // Apply style filter - photographer must have ALL specified styles
            if (styleIds != null && styleIds.Any())
            {
                foreach (var styleId in styleIds)
                {
                    var currentStyleId = styleId; // Capture for closure
                    query = query.Where(u => u.PhotographerProfile!.PhotographerStyles.Any(ps => ps.StyleId == currentStyleId));
                }
            }

            // Apply date range filters
            if (createdFrom.HasValue)
            {
                query = query.Where(u => u.CreatedAt >= createdFrom.Value);
            }

            if (createdTo.HasValue)
            {
                query = query.Where(u => u.CreatedAt <= createdTo.Value);
            }

            // Apply sorting
            var isDescending = sortDirection?.ToLower() == "desc";
            
            if (!string.IsNullOrEmpty(sortBy))
            {
                query = sortBy.ToLower() switch
                {
                    "name" => isDescending ? query.OrderByDescending(u => u.Name) : query.OrderBy(u => u.Name),
                    "email" => isDescending ? query.OrderByDescending(u => u.Email) : query.OrderBy(u => u.Email),
                    "rating" => isDescending ? query.OrderByDescending(u => u.PhotographerProfile!.AvgRating ?? 0) : query.OrderBy(u => u.PhotographerProfile!.AvgRating ?? 0),
                    "createdat" => isDescending ? query.OrderByDescending(u => u.CreatedAt) : query.OrderBy(u => u.CreatedAt),
                    "yearsofexperience" => isDescending ? query.OrderByDescending(u => u.PhotographerProfile!.YearsOfExperience ?? "") : query.OrderBy(u => u.PhotographerProfile!.YearsOfExperience ?? ""),
                    "worklocation" => isDescending ? query.OrderByDescending(u => u.PhotographerProfile!.WorkLocation ?? "") : query.OrderBy(u => u.PhotographerProfile!.WorkLocation ?? ""),
                    _ => isDescending ? query.OrderByDescending(u => u.CreatedAt) : query.OrderBy(u => u.CreatedAt)
                };
            }
            else
            {
                query = isDescending ? query.OrderByDescending(u => u.CreatedAt) : query.OrderBy(u => u.CreatedAt);
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync();

            // Apply pagination
            var photographers = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (photographers, totalCount);
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