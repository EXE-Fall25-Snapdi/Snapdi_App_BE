using Snapdi.Services.DTOs;

namespace Snapdi.Services.Interfaces
{
    public interface IPhotographerPhotoTypeService
  {
      Task<IEnumerable<PhotoTypeWithPricingDto>> GetByUserIdAsync(int userId);
        Task<PhotoTypeWithPricingDto?> GetByUserIdAndPhotoTypeIdAsync(int userId, int photoTypeId);
  Task<PhotoTypeWithPricingDto> AddAsync(int userId, PhotoTypeWithPricingDto photoTypeDto);
        Task<IEnumerable<PhotoTypeWithPricingDto>> AddMultipleAsync(int userId, IEnumerable<PhotoTypeWithPricingDto> photoTypeDtos);
   Task<PhotoTypeWithPricingDto?> UpdateAsync(int userId, int photoTypeId, PhotoTypeWithPricingDto photoTypeDto);
        Task<bool> DeleteAsync(int userId, int photoTypeId);
  Task<bool> DeleteAllByUserIdAsync(int userId);
     Task<bool> ExistsAsync(int userId, int photoTypeId);
    Task<bool> UpdatePhotographerPhotoTypesAsync(int userId, IEnumerable<PhotoTypeWithPricingDto> photoTypeDtos);
    }
}
