using Snapdi.Repositories.Models;

namespace Snapdi.Repositories.Interfaces
{
    public interface IUserRepository : IBaseRepository<User>
    {
        Task<User?> GetAdminUserAsync();
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
        
        // New method for filtering and paging
        Task<(IEnumerable<User> Users, int TotalCount)> GetUsersWithFilterAsync(
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
            DateTime? createdTo = null);
        
        // Photographer search method
        Task<(IEnumerable<User> Photographers, int TotalCount)> SearchPhotographersAsync(
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
            List<int>? photoTypeIds = null,
            double? minPrice = null,
            double? maxPrice = null,
            DateTime? createdFrom = null,
            DateTime? createdTo = null,
            string? sortBy = "createdAt",
            string? sortDirection = "desc");
            
        // Get photographers pending level assignment
        Task<IEnumerable<User>> GetPhotographersPendingLevelAssignmentAsync();
        
        // Get photographers pending level assignment with paging and filtering
        Task<(IEnumerable<User> WithPortfolio, IEnumerable<User> WithoutPortfolio, int WithPortfolioTotalCount, int WithoutPortfolioTotalCount)> 
            GetPhotographersPendingLevelAssignmentPagedAsync(
                int page,
                int pageSize,
                string? searchTerm = null,
                bool? hasPortfolio = null,
                string? locationCity = null,
                string? sortBy = "createdAt",
                string? sortDirection = "desc",
                DateTime? createdFrom = null,
                DateTime? createdTo = null
            );
        
        // Update photographer level
        Task UpdatePhotographerLevelAsync(int userId, string levelPhotographer);
    }
}