using Snapdi.Services.DTOs;

namespace Snapdi.Services.Interfaces;

public interface IStyleService
{
    Task<IEnumerable<StyleDto>> GetAllStylesAsync();
    Task<StyleDto?> GetStyleByIdAsync(int styleId);
    Task<StyleDto?> GetStyleByNameAsync(string styleName);
    Task<StyleDto> CreateStyleAsync(CreateStyleDto createStyleDto);
    Task<StyleDto?> UpdateStyleAsync(int styleId, UpdateStyleDto updateStyleDto);
    Task<bool> DeleteStyleAsync(int styleId);
    Task<bool> IsStyleNameExistsAsync(string styleName, int? excludeId = null);
}
