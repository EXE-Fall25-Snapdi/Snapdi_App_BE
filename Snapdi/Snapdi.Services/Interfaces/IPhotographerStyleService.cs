using Snapdi.Services.DTOs;

namespace Snapdi.Services.Interfaces;

public interface IPhotographerStyleService
{
    Task<IEnumerable<StyleDto>> GetStylesByPhotographerAsync(int userId);
    Task<PhotographerStyleResponseDto?> GetPhotographerWithStylesAsync(int userId);
    Task<IEnumerable<StyleSelectionDto>> GetStyleSelectionForPhotographerAsync(int userId);
    Task<bool> AddStyleToPhotographerAsync(int userId, int styleId);
    Task<bool> AddMultipleStylesToPhotographerAsync(int userId, IEnumerable<int> styleIds);
    Task<bool> RemoveStyleFromPhotographerAsync(int userId, int styleId);
    Task<bool> UpdatePhotographerStylesAsync(int userId, IEnumerable<int> styleIds);
    Task<bool> UpdatePhotographerStylesDifferentialAsync(int userId, IEnumerable<int> styleIds);
    Task<bool> IsPhotographerStyleExistsAsync(int userId, int styleId);
    Task<IEnumerable<PhotographerStyleDto>> GetPhotographersByStyleAsync(int styleId);
}
