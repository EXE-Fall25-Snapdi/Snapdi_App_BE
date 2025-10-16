using Snapdi.Repositories.Interfaces;
using Snapdi.Repositories.Models;
using Snapdi.Services.DTOs;
using Snapdi.Services.Interfaces;

namespace Snapdi.Services.Services
{
    public class PhotoPortfolioService : IPhotoPortfolioService
    {
        private readonly IPhotoPortfolioRepository _photoPortfolioRepository;
        private readonly IUserRepository _userRepository;

        public PhotoPortfolioService(
            IPhotoPortfolioRepository photoPortfolioRepository,
            IUserRepository userRepository)
        {
            _photoPortfolioRepository = photoPortfolioRepository;
            _userRepository = userRepository;
        }

        public async Task<IEnumerable<PhotoPortfolioDto>> GetPhotoPortfoliosByUserIdAsync(int userId)
        {
            // Verify user exists
            var userExists = await _userRepository.ExistsAsync(u => u.UserId == userId);
            if (!userExists)
            {
                return Enumerable.Empty<PhotoPortfolioDto>();
            }

            var portfolios = await _photoPortfolioRepository.GetByUserIdAsync(userId);
            return portfolios.Select(MapToPhotoPortfolioDto);
        }

        public async Task<PhotoPortfolioDto?> GetPhotoPortfolioByIdAsync(int photoPortfolioId)
        {
            var portfolio = await _photoPortfolioRepository.GetByIdAsync(photoPortfolioId);
            return portfolio != null ? MapToPhotoPortfolioDto(portfolio) : null;
        }

        public async Task<PhotoPortfolioDto?> GetPhotoPortfolioByIdAndUserIdAsync(int photoPortfolioId, int userId)
        {
            var portfolio = await _photoPortfolioRepository.GetByIdAndUserIdAsync(photoPortfolioId, userId);
            return portfolio != null ? MapToPhotoPortfolioDto(portfolio) : null;
        }

        public async Task<PhotoPortfolioDto> CreatePhotoPortfolioAsync(int userId, CreatePhotoPortfolioDto createDto)
        {
            // Verify user exists
            var userExists = await _userRepository.ExistsAsync(u => u.UserId == userId);
            if (!userExists)
            {
                throw new InvalidOperationException($"User with ID {userId} does not exist.");
            }

            var portfolio = new PhotoPortfolio
            {
                UserId = userId,
                PhotoUrl = createDto.PhotoUrl
            };

            var createdPortfolio = await _photoPortfolioRepository.AddAsync(portfolio);
            await _photoPortfolioRepository.SaveChangesAsync();

            return MapToPhotoPortfolioDto(createdPortfolio);
        }

        public async Task<PhotoPortfolioDto?> UpdatePhotoPortfolioAsync(int photoPortfolioId, UpdatePhotoPortfolioDto updateDto)
        {
            var portfolio = await _photoPortfolioRepository.GetByIdAsync(photoPortfolioId);
            if (portfolio == null)
                return null;

            if (!string.IsNullOrEmpty(updateDto.PhotoUrl))
            {
                portfolio.PhotoUrl = updateDto.PhotoUrl;
            }

            await _photoPortfolioRepository.UpdateAsync(portfolio);
            await _photoPortfolioRepository.SaveChangesAsync();

            return MapToPhotoPortfolioDto(portfolio);
        }

        public async Task<PhotoPortfolioDto?> UpdatePhotoPortfolioByUserAsync(int photoPortfolioId, int userId, UpdatePhotoPortfolioDto updateDto)
        {
            var portfolio = await _photoPortfolioRepository.GetByIdAndUserIdAsync(photoPortfolioId, userId);
            if (portfolio == null)
                return null;

            if (!string.IsNullOrEmpty(updateDto.PhotoUrl))
            {
                portfolio.PhotoUrl = updateDto.PhotoUrl;
            }

            await _photoPortfolioRepository.UpdateAsync(portfolio);
            await _photoPortfolioRepository.SaveChangesAsync();

            return MapToPhotoPortfolioDto(portfolio);
        }

        public async Task<bool> DeletePhotoPortfolioAsync(int photoPortfolioId)
        {
            var portfolio = await _photoPortfolioRepository.GetByIdAsync(photoPortfolioId);
            if (portfolio == null)
                return false;

            await _photoPortfolioRepository.DeleteAsync(portfolio);
            await _photoPortfolioRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeletePhotoPortfolioByUserAsync(int photoPortfolioId, int userId)
        {
            var portfolio = await _photoPortfolioRepository.GetByIdAndUserIdAsync(photoPortfolioId, userId);
            if (portfolio == null)
                return false;

            await _photoPortfolioRepository.DeleteAsync(portfolio);
            await _photoPortfolioRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAllPhotoPortfoliosByUserIdAsync(int userId)
        {
            // Verify user exists
            var userExists = await _userRepository.ExistsAsync(u => u.UserId == userId);
            if (!userExists)
                return false;

            await _photoPortfolioRepository.DeleteAllByUserIdAsync(userId);
            await _photoPortfolioRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UserHasPortfolioAsync(int userId)
        {
            return await _photoPortfolioRepository.ExistsByUserIdAsync(userId);
        }

        public async Task<int> GetPortfolioCountByUserIdAsync(int userId)
        {
            return await _photoPortfolioRepository.CountByUserIdAsync(userId);
        }

        #region Private Methods

        private static PhotoPortfolioDto MapToPhotoPortfolioDto(PhotoPortfolio portfolio)
        {
            return new PhotoPortfolioDto
            {
                PhotoPortfolioId = portfolio.PhotoPortfolioId,
                UserId = portfolio.UserId,
                PhotoUrl = portfolio.PhotoUrl
            };
        }

        #endregion
    }
}