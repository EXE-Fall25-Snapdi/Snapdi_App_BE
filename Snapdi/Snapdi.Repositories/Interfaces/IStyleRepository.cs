using Snapdi.Repositories.Models;

namespace Snapdi.Repositories.Interfaces;

public interface IStyleRepository : IBaseRepository<Style>
{
    Task<IEnumerable<Style>> GetAllStylesAsync();
    Task<Style?> GetStyleByIdAsync(int styleId);
    Task<Style?> GetStyleByNameAsync(string styleName);
    Task<bool> IsStyleNameExistsAsync(string styleName, int? excludeId = null);
}
