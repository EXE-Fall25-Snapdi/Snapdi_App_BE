using Snapdi.Repositories.Interfaces;
using Snapdi.Services.DTOs;
using Snapdi.Services.Interfaces;

namespace Snapdi.Services.Services;

public class StyleService : IStyleService
{
    private readonly IStyleRepository _styleRepository;

    public StyleService(IStyleRepository styleRepository)
    {
        _styleRepository = styleRepository;
    }

    public async Task<IEnumerable<StyleDto>> GetAllStylesAsync()
    {
        var styles = await _styleRepository.GetAllStylesAsync();
        return styles.Select(MapToStyleDto);
    }

    public async Task<StyleDto?> GetStyleByIdAsync(int styleId)
    {
        var style = await _styleRepository.GetStyleByIdAsync(styleId);
        return style != null ? MapToStyleDto(style) : null;
    }

    public async Task<StyleDto?> GetStyleByNameAsync(string styleName)
    {
        var style = await _styleRepository.GetStyleByNameAsync(styleName);
        return style != null ? MapToStyleDto(style) : null;
    }

    public async Task<StyleDto> CreateStyleAsync(CreateStyleDto createStyleDto)
    {
        // Check if style name already exists
        if (await _styleRepository.IsStyleNameExistsAsync(createStyleDto.StyleName))
        {
            throw new InvalidOperationException($"Style with name '{createStyleDto.StyleName}' already exists.");
        }

        var style = new Snapdi.Repositories.Models.Style
        {
            StyleName = createStyleDto.StyleName
        };

        await _styleRepository.AddAsync(style);
        await _styleRepository.SaveChangesAsync();

        return MapToStyleDto(style);
    }

    public async Task<StyleDto?> UpdateStyleAsync(int styleId, UpdateStyleDto updateStyleDto)
    {
        var style = await _styleRepository.GetStyleByIdAsync(styleId);
        if (style == null)
        {
            return null;
        }

        // Check if new name already exists (excluding current style)
        if (await _styleRepository.IsStyleNameExistsAsync(updateStyleDto.StyleName, styleId))
        {
            throw new InvalidOperationException($"Style with name '{updateStyleDto.StyleName}' already exists.");
        }

        style.StyleName = updateStyleDto.StyleName;
        await _styleRepository.UpdateAsync(style);
        await _styleRepository.SaveChangesAsync();

        return MapToStyleDto(style);
    }

    public async Task<bool> DeleteStyleAsync(int styleId)
    {
        var style = await _styleRepository.GetStyleByIdAsync(styleId);
        if (style == null)
        {
            return false;
        }

        await _styleRepository.DeleteAsync(style);
        await _styleRepository.SaveChangesAsync();
        return true;
    }

    public async Task<bool> IsStyleNameExistsAsync(string styleName, int? excludeId = null)
    {
        return await _styleRepository.IsStyleNameExistsAsync(styleName, excludeId);
    }

    private static StyleDto MapToStyleDto(Snapdi.Repositories.Models.Style style)
    {
        return new StyleDto
        {
            StyleId = style.StyleId,
            StyleName = style.StyleName
        };
    }
}
