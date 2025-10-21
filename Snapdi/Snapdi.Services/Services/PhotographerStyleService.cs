using Snapdi.Repositories.Interfaces;
using Snapdi.Services.DTOs;
using Snapdi.Services.Interfaces;

namespace Snapdi.Services.Services;

public class PhotographerStyleService : IPhotographerStyleService
{
    private readonly IPhotographerStyleRepository _photographerStyleRepository;
    private readonly IStyleRepository _styleRepository;
    private readonly IUserRepository _userRepository;

    public PhotographerStyleService(
        IPhotographerStyleRepository photographerStyleRepository,
        IStyleRepository styleRepository,
        IUserRepository userRepository)
    {
        _photographerStyleRepository = photographerStyleRepository;
        _styleRepository = styleRepository;
        _userRepository = userRepository;
    }

    public async Task<IEnumerable<StyleDto>> GetStylesByPhotographerAsync(int userId)
    {
        var styles = await _photographerStyleRepository.GetStylesByPhotographerAsync(userId);
        return styles.Select(MapToStyleDto);
    }

    public async Task<PhotographerStyleResponseDto?> GetPhotographerWithStylesAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return null;
        }

        var styles = await _photographerStyleRepository.GetStylesByPhotographerAsync(userId);
        var styleDtos = styles.Select(MapToStyleDto).ToList();

        return new PhotographerStyleResponseDto
        {
            UserId = userId,
            Name = user.Name,
            SelectedStyles = styleDtos
        };
    }

    public async Task<IEnumerable<StyleSelectionDto>> GetStyleSelectionForPhotographerAsync(int userId)
    {
        var allStyles = await _styleRepository.GetAllStylesAsync();
        var selectedStyleIds = (await _photographerStyleRepository.GetStylesByPhotographerAsync(userId))
            .Select(s => s.StyleId)
            .ToHashSet();

        return allStyles.Select(style => new StyleSelectionDto
        {
            StyleId = style.StyleId,
            StyleName = style.StyleName,
            IsSelected = selectedStyleIds.Contains(style.StyleId)
        });
    }

    public async Task<bool> AddStyleToPhotographerAsync(int userId, int styleId)
    {
        return await _photographerStyleRepository.AddPhotographerStyleAsync(userId, styleId);
    }

    public async Task<bool> AddMultipleStylesToPhotographerAsync(int userId, IEnumerable<int> styleIds)
    {
        return await _photographerStyleRepository.AddMultiplePhotographerStylesAsync(userId, styleIds);
    }

    public async Task<bool> RemoveStyleFromPhotographerAsync(int userId, int styleId)
    {
        return await _photographerStyleRepository.RemovePhotographerStyleAsync(userId, styleId);
    }

    public async Task<bool> UpdatePhotographerStylesAsync(int userId, IEnumerable<int> styleIds)
    {
        return await _photographerStyleRepository.UpdatePhotographerStylesAsync(userId, styleIds);
    }

    public async Task<bool> UpdatePhotographerStylesDifferentialAsync(int userId, IEnumerable<int> styleIds)
    {
        return await _photographerStyleRepository.UpdatePhotographerStylesDifferentialAsync(userId, styleIds);
    }

    public async Task<bool> IsPhotographerStyleExistsAsync(int userId, int styleId)
    {
        return await _photographerStyleRepository.IsPhotographerStyleExistsAsync(userId, styleId);
    }

    public async Task<IEnumerable<PhotographerStyleDto>> GetPhotographersByStyleAsync(int styleId)
    {
        var photographerStyles = await _photographerStyleRepository.GetPhotographersByStyleAsync(styleId);
        return photographerStyles.Select(ps => new PhotographerStyleDto
        {
            UserId = ps.UserId,
            StyleId = ps.StyleId,
            StyleName = ps.Style.StyleName,
            Name = ps.PhotographerProfile.User.Name
        });
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
