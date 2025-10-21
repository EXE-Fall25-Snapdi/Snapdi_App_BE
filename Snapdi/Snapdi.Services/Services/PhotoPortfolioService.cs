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

        public async Task<CreateMultiplePhotoPortfolioResponseDto> CreateMultiplePhotoPortfoliosAsync(int userId, CreateMultiplePhotoPortfolioDto createDto)
        {
            var response = new CreateMultiplePhotoPortfolioResponseDto
            {
                TotalAttempted = createDto.PhotoUrls.Count
            };

            // Verify user exists
            var userExists = await _userRepository.ExistsAsync(u => u.UserId == userId);
            if (!userExists)
            {
                throw new InvalidOperationException($"User with ID {userId} does not exist.");
            }

            // Filter out null, empty, or whitespace photo URLs and remove duplicates
            var validPhotoUrls = createDto.PhotoUrls
                .Where(url => !string.IsNullOrWhiteSpace(url))
                .Select(url => url.Trim())
                .Distinct()
                .ToList();

            if (!validPhotoUrls.Any())
            {
                throw new ArgumentException("No valid photo URLs provided.");
            }

            // Update total attempted to reflect only valid URLs
            response.TotalAttempted = validPhotoUrls.Count;

            var portfolios = new List<PhotoPortfolio>();

            // Create portfolio entities for all valid URLs
            foreach (var photoUrl in validPhotoUrls)
            {
                try
                {
                    var portfolio = new PhotoPortfolio
                    {
                        UserId = userId,
                        PhotoUrl = photoUrl
                    };
                    portfolios.Add(portfolio);
                }
                catch
                {
                    response.FailedPhotoUrls.Add(photoUrl);
                }
            }

            // Bulk insert all portfolios
            try
            {
                var createdPortfolios = await _photoPortfolioRepository.AddRangeAsync(portfolios);
                await _photoPortfolioRepository.SaveChangesAsync();

                response.CreatedPortfolios = createdPortfolios.Select(MapToPhotoPortfolioDto).ToList();
                response.SuccessCount = response.CreatedPortfolios.Count;
                response.FailedCount = response.FailedPhotoUrls.Count;

                if (response.IsCompleteSuccess)
                {
                    response.Message = $"Successfully created {response.SuccessCount} photo portfolios.";
                }
                else
                {
                    response.Message = $"Created {response.SuccessCount} photo portfolios successfully. {response.FailedCount} failed to create.";
                }
            }
            catch (Exception ex)
            {
                // If bulk insert fails, try individual inserts to identify which ones failed
                response.CreatedPortfolios.Clear();
                response.FailedPhotoUrls.Clear();

                foreach (var portfolio in portfolios)
                {
                    try
                    {
                        var createdPortfolio = await _photoPortfolioRepository.AddAsync(portfolio);
                        await _photoPortfolioRepository.SaveChangesAsync();
                        response.CreatedPortfolios.Add(MapToPhotoPortfolioDto(createdPortfolio));
                    }
                    catch
                    {
                        response.FailedPhotoUrls.Add(portfolio.PhotoUrl);
                    }
                }

                response.SuccessCount = response.CreatedPortfolios.Count;
                response.FailedCount = response.FailedPhotoUrls.Count;
                response.Message = $"Bulk insert failed. Created {response.SuccessCount} photo portfolios individually. {response.FailedCount} failed to create.";
            }

            return response;
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