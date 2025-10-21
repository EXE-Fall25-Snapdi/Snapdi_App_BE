using Snapdi.Repositories.Models;

namespace Snapdi.Repositories.Interfaces;

public interface IPhotographerStyleRepository : IBaseRepository<PhotographerStyle>
{
    Task<IEnumerable<PhotographerStyle>> GetPhotographerStylesAsync(int userId);
    Task<IEnumerable<Style>> GetStylesByPhotographerAsync(int userId);
    Task<IEnumerable<PhotographerStyle>> GetPhotographersByStyleAsync(int styleId);
    Task<bool> IsPhotographerStyleExistsAsync(int userId, int styleId);
    Task<PhotographerStyle?> GetPhotographerStyleAsync(int userId, int styleId);
    Task<bool> AddPhotographerStyleAsync(int userId, int styleId);
    Task<bool> AddMultiplePhotographerStylesAsync(int userId, IEnumerable<int> styleIds);
    Task<bool> RemovePhotographerStyleAsync(int userId, int styleId);
    Task<bool> UpdatePhotographerStylesAsync(int userId, IEnumerable<int> styleIds);
    Task<bool> UpdatePhotographerStylesDifferentialAsync(int userId, IEnumerable<int> newStyleIds);
}
