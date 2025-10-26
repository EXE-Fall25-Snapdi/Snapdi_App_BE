using Snapdi.Repositories.Models;

namespace Snapdi.Repositories.Interfaces
{
    public interface IPhotographerProfileRepository : IBaseRepository<PhotographerProfile>
    {
        Task<PhotographerProfile?> GetByUserIdAsync(int userId);
        Task<IEnumerable<PhotographerProfile>> GetAvailablePhotographersAsync();
        Task<IEnumerable<PhotographerProfile>> GetPhotographersByCityAsync(string city);
        Task<bool> ExistsByUserIdAsync(int userId);
        Task UpdatePhotographerStatusAsync(int userId, bool isAvailable);
    }
}