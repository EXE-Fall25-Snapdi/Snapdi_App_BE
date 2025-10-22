using Snapdi.Services.DTOs;

namespace Snapdi.Services.Interfaces
{
    public interface IPhotoPortfolioService
    {
        Task<IEnumerable<PhotoPortfolioDto>> GetPhotoPortfoliosByUserIdAsync(int userId);
        Task<PhotoPortfolioDto?> GetPhotoPortfolioByIdAsync(int photoPortfolioId);
        Task<PhotoPortfolioDto?> GetPhotoPortfolioByIdAndUserIdAsync(int photoPortfolioId, int userId);
        Task<PhotoPortfolioDto> CreatePhotoPortfolioAsync(int userId, CreatePhotoPortfolioDto createDto);
        Task<CreateMultiplePhotoPortfolioResponseDto> CreateMultiplePhotoPortfoliosAsync(int userId, CreateMultiplePhotoPortfolioDto createDto);
        Task<PhotoPortfolioDto?> UpdatePhotoPortfolioAsync(int photoPortfolioId, UpdatePhotoPortfolioDto updateDto);
        Task<PhotoPortfolioDto?> UpdatePhotoPortfolioByUserAsync(int photoPortfolioId, int userId, UpdatePhotoPortfolioDto updateDto);
        Task<bool> DeletePhotoPortfolioAsync(int photoPortfolioId);
        Task<bool> DeletePhotoPortfolioByUserAsync(int photoPortfolioId, int userId);
        Task<bool> DeleteAllPhotoPortfoliosByUserIdAsync(int userId);
        Task<bool> UserHasPortfolioAsync(int userId);
        Task<int> GetPortfolioCountByUserIdAsync(int userId);
    }
}