using Snapdi.Repositories.Interfaces;
using Snapdi.Repositories.Models;
using Snapdi.Services.DTOs;
using Snapdi.Services.Interfaces;

namespace Snapdi.Services.Services
{
    public class PhotographerPhotoTypeService : IPhotographerPhotoTypeService
    {
        private readonly IPhotographerPhotoTypeRepository _photographerPhotoTypeRepository;
        private readonly IPhotoTypeRepository _photoTypeRepository;
        private readonly IPhotographerProfileRepository _photographerProfileRepository;

        public PhotographerPhotoTypeService(
          IPhotographerPhotoTypeRepository photographerPhotoTypeRepository,
          IPhotoTypeRepository photoTypeRepository,
                IPhotographerProfileRepository photographerProfileRepository)
        {
            _photographerPhotoTypeRepository = photographerPhotoTypeRepository;
            _photoTypeRepository = photoTypeRepository;
            _photographerProfileRepository = photographerProfileRepository;
        }

        public async Task<IEnumerable<PhotoTypeWithPricingDto>> GetByUserIdAsync(int userId)
        {
            var photographerPhotoTypes = await _photographerPhotoTypeRepository.GetByUserIdAsync(userId);
            return photographerPhotoTypes.Select(MapToDto);
        }

        public async Task<PhotoTypeWithPricingDto?> GetByUserIdAndPhotoTypeIdAsync(int userId, int photoTypeId)
        {
            var photographerPhotoType = await _photographerPhotoTypeRepository.GetByUserIdAndPhotoTypeIdAsync(userId, photoTypeId);
            return photographerPhotoType != null ? MapToDto(photographerPhotoType) : null;
        }

        public async Task<PhotoTypeWithPricingDto> AddAsync(int userId, PhotoTypeWithPricingDto photoTypeDto)
        {
            // Verify photographer profile exists
            var photographerProfile = await _photographerProfileRepository.GetByUserIdAsync(userId);
            if (photographerProfile == null)
            {
                throw new InvalidOperationException($"Photographer profile for user ID {userId} does not exist.");
            }

            // Verify photo type exists
            var photoType = await _photoTypeRepository.GetPhotoTypeByIdAsync(photoTypeDto.PhotoTypeId);
            if (photoType == null)
            {
                throw new InvalidOperationException($"Photo type with ID {photoTypeDto.PhotoTypeId} does not exist.");
            }

            // Check if association already exists
            if (await _photographerPhotoTypeRepository.ExistsByUserIdAndPhotoTypeIdAsync(userId, photoTypeDto.PhotoTypeId))
            {
                throw new InvalidOperationException($"Photographer already has photo type with ID {photoTypeDto.PhotoTypeId}.");
            }

            var photographerPhotoType = new PhotographerPhotoType
            {
                UserId = userId,
                PhotoTypeId = photoTypeDto.PhotoTypeId,
                PhotoPrice = photoTypeDto.PhotoPrice,
                Time = photoTypeDto.Time
            };

            var created = await _photographerPhotoTypeRepository.AddAsync(photographerPhotoType);
            await _photographerPhotoTypeRepository.SaveChangesAsync();

            // Reload with navigation properties
            var result = await _photographerPhotoTypeRepository.GetByUserIdAndPhotoTypeIdAsync(userId, photoTypeDto.PhotoTypeId);
            return MapToDto(result!);
        }

        public async Task<IEnumerable<PhotoTypeWithPricingDto>> AddMultipleAsync(int userId, IEnumerable<PhotoTypeWithPricingDto> photoTypeDtos)
        {
            // Verify photographer profile exists
            var photographerProfile = await _photographerProfileRepository.GetByUserIdAsync(userId);
            if (photographerProfile == null)
            {
                throw new InvalidOperationException($"Photographer profile for user ID {userId} does not exist.");
            }

            var photographerPhotoTypes = new List<PhotographerPhotoType>();

            foreach (var photoTypeDto in photoTypeDtos)
            {
                // Verify photo type exists
                var photoType = await _photoTypeRepository.GetPhotoTypeByIdAsync(photoTypeDto.PhotoTypeId);
                if (photoType == null)
                {
                    throw new InvalidOperationException($"Photo type with ID {photoTypeDto.PhotoTypeId} does not exist.");
                }

                // Skip if association already exists
                if (await _photographerPhotoTypeRepository.ExistsByUserIdAndPhotoTypeIdAsync(userId, photoTypeDto.PhotoTypeId))
                {
                    continue;
                }

                photographerPhotoTypes.Add(new PhotographerPhotoType
                {
                    UserId = userId,
                    PhotoTypeId = photoTypeDto.PhotoTypeId,
                    PhotoPrice = photoTypeDto.PhotoPrice,
                    Time = photoTypeDto.Time
                });
            }

            if (photographerPhotoTypes.Any())
            {
                await _photographerPhotoTypeRepository.AddRangeAsync(photographerPhotoTypes);
                await _photographerPhotoTypeRepository.SaveChangesAsync();
            }

            // Reload all for the user
            return await GetByUserIdAsync(userId);
        }

        public async Task<PhotoTypeWithPricingDto?> UpdateAsync(int userId, int photoTypeId, PhotoTypeWithPricingDto photoTypeDto)
        {
            var photographerPhotoType = await _photographerPhotoTypeRepository.GetByUserIdAndPhotoTypeIdAsync(userId, photoTypeId);
            if (photographerPhotoType == null)
            {
                return null;
            }

            photographerPhotoType.PhotoPrice = photoTypeDto.PhotoPrice;
            photographerPhotoType.Time = photoTypeDto.Time;

            await _photographerPhotoTypeRepository.UpdateAsync(photographerPhotoType);
            await _photographerPhotoTypeRepository.SaveChangesAsync();

            // Reload with navigation properties
            var result = await _photographerPhotoTypeRepository.GetByUserIdAndPhotoTypeIdAsync(userId, photoTypeId);
            return MapToDto(result!);
        }

        public async Task<bool> DeleteAsync(int userId, int photoTypeId)
        {
            if (!await _photographerPhotoTypeRepository.ExistsByUserIdAndPhotoTypeIdAsync(userId, photoTypeId))
            {
                return false;
            }

            await _photographerPhotoTypeRepository.DeleteByUserIdAndPhotoTypeIdAsync(userId, photoTypeId);
            await _photographerPhotoTypeRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAllByUserIdAsync(int userId)
        {
            await _photographerPhotoTypeRepository.DeleteByUserIdAsync(userId);
            await _photographerPhotoTypeRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int userId, int photoTypeId)
        {
            return await _photographerPhotoTypeRepository.ExistsByUserIdAndPhotoTypeIdAsync(userId, photoTypeId);
        }

        public async Task<bool> UpdatePhotographerPhotoTypesAsync(int userId, IEnumerable<PhotoTypeWithPricingDto> photoTypeDtos)
        {
            // Verify photographer profile exists
            var photographerProfile = await _photographerProfileRepository.GetByUserIdAsync(userId);
            if (photographerProfile == null)
            {
                return false;
            }

            // Get existing photo types
            var existingPhotoTypes = await _photographerPhotoTypeRepository.GetByUserIdAsync(userId);
            var existingPhotoTypeIds = existingPhotoTypes.Select(ppt => ppt.PhotoTypeId).ToList();
            var newPhotoTypeIds = photoTypeDtos.Select(pt => pt.PhotoTypeId).ToList();

            // Remove photo types that are no longer in the list
            var toRemove = existingPhotoTypes.Where(ppt => !newPhotoTypeIds.Contains(ppt.PhotoTypeId));
            foreach (var item in toRemove)
            {
                await _photographerPhotoTypeRepository.DeleteAsync(item);
            }

            // Add or update photo types
            foreach (var photoTypeDto in photoTypeDtos)
            {
                var existing = existingPhotoTypes.FirstOrDefault(ppt => ppt.PhotoTypeId == photoTypeDto.PhotoTypeId);
                if (existing != null)
                {
                    // Update existing
                    existing.PhotoPrice = photoTypeDto.PhotoPrice;
                    existing.Time = photoTypeDto.Time;
                    await _photographerPhotoTypeRepository.UpdateAsync(existing);
                }
                else
                {
                    // Add new
                    // Verify photo type exists
                    var photoType = await _photoTypeRepository.GetPhotoTypeByIdAsync(photoTypeDto.PhotoTypeId);
                    if (photoType == null)
                    {
                        continue; // Skip invalid photo types
                    }

                    var photographerPhotoType = new PhotographerPhotoType
                    {
                        UserId = userId,
                        PhotoTypeId = photoTypeDto.PhotoTypeId,
                        PhotoPrice = photoTypeDto.PhotoPrice,
                        Time = photoTypeDto.Time
                    };
                    await _photographerPhotoTypeRepository.AddAsync(photographerPhotoType);
                }
            }

            await _photographerPhotoTypeRepository.SaveChangesAsync();
            return true;
        }

        private static PhotoTypeWithPricingDto MapToDto(PhotographerPhotoType photographerPhotoType)
        {
            return new PhotoTypeWithPricingDto
            {
                PhotoTypeId = photographerPhotoType.PhotoTypeId,
                PhotoPrice = photographerPhotoType.PhotoPrice,
                Time = photographerPhotoType.Time
            };
        }
    }
}
