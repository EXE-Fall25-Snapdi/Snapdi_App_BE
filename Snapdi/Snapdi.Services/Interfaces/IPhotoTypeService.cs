using Snapdi.Services.DTOs;

namespace Snapdi.Services.Interfaces;

public interface IPhotoTypeService
{
    Task<IEnumerable<PhotoTypeDto>> GetAllPhotoTypesAsync();
    Task<PhotoTypeDto?> GetPhotoTypeByIdAsync(int photoTypeId);
    Task<PhotoTypeDto?> GetPhotoTypeByNameAsync(string photoTypeName);
    Task<PhotoTypeDto> CreatePhotoTypeAsync(CreatePhotoTypeDto createPhotoTypeDto);
    Task<PhotoTypeDto?> UpdatePhotoTypeAsync(int photoTypeId, UpdatePhotoTypeDto updatePhotoTypeDto);
    Task<bool> DeletePhotoTypeAsync(int photoTypeId);
    Task<bool> IsPhotoTypeNameExistsAsync(string photoTypeName, int? excludeId = null);
}
