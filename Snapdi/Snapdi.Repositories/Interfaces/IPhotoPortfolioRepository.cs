using Snapdi.Repositories.Models;

namespace Snapdi.Repositories.Interfaces
{
    public interface IPhotoPortfolioRepository : IBaseRepository<PhotoPortfolio>
    {
        Task<IEnumerable<PhotoPortfolio>> GetByUserIdAsync(int userId);
        Task<PhotoPortfolio?> GetByIdAndUserIdAsync(int photoPortfolioId, int userId);
        Task<bool> ExistsByUserIdAsync(int userId);
        Task<int> CountByUserIdAsync(int userId);
        Task DeleteAllByUserIdAsync(int userId);
    }
}