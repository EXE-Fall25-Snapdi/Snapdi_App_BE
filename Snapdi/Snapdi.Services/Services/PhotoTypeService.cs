using Snapdi.Repositories.Interfaces;
using Snapdi.Services.DTOs;
using Snapdi.Services.Interfaces;

namespace Snapdi.Services.Services;

public class PhotoTypeService : IPhotoTypeService
{
    private readonly IPhotoTypeRepository _photoTypeRepository;

    public PhotoTypeService(IPhotoTypeRepository photoTypeRepository)
    {
        _photoTypeRepository = photoTypeRepository;
    }

    public async Task<IEnumerable<PhotoTypeDto>> GetAllPhotoTypesAsync()
    {
        var photoTypes = await _photoTypeRepository.GetAllPhotoTypesAsync();
        return photoTypes.Select(MapToPhotoTypeDto);
    }

    public async Task<PhotoTypeDto?> GetPhotoTypeByIdAsync(int photoTypeId)
    {
        var photoType = await _photoTypeRepository.GetPhotoTypeByIdAsync(photoTypeId);
        return photoType != null ? MapToPhotoTypeDto(photoType) : null;
    }

    public async Task<PhotoTypeDto?> GetPhotoTypeByNameAsync(string photoTypeName)
    {
        var photoType = await _photoTypeRepository.GetPhotoTypeByNameAsync(photoTypeName);
        return photoType != null ? MapToPhotoTypeDto(photoType) : null;
    }

    public async Task<PhotoTypeDto> CreatePhotoTypeAsync(CreatePhotoTypeDto createPhotoTypeDto)
    {
        // Check if photo type name already exists
        if (await _photoTypeRepository.IsPhotoTypeNameExistsAsync(createPhotoTypeDto.PhotoTypeName))
        {
            throw new InvalidOperationException($"Photo type with name '{createPhotoTypeDto.PhotoTypeName}' already exists.");
        }

        var photoType = new Snapdi.Repositories.Models.PhotoType
        {
            PhotoTypeName = createPhotoTypeDto.PhotoTypeName
        };

        await _photoTypeRepository.AddAsync(photoType);
        await _photoTypeRepository.SaveChangesAsync();

        return MapToPhotoTypeDto(photoType);
    }

    public async Task<PhotoTypeDto?> UpdatePhotoTypeAsync(int photoTypeId, UpdatePhotoTypeDto updatePhotoTypeDto)
    {
        var photoType = await _photoTypeRepository.GetPhotoTypeByIdAsync(photoTypeId);
        if (photoType == null)
        {
            return null;
        }

        // Check if new name already exists (excluding current photo type)
        if (await _photoTypeRepository.IsPhotoTypeNameExistsAsync(updatePhotoTypeDto.PhotoTypeName, photoTypeId))
        {
            throw new InvalidOperationException($"Photo type with name '{updatePhotoTypeDto.PhotoTypeName}' already exists.");
        }

        photoType.PhotoTypeName = updatePhotoTypeDto.PhotoTypeName;
        await _photoTypeRepository.UpdateAsync(photoType);
        await _photoTypeRepository.SaveChangesAsync();

        return MapToPhotoTypeDto(photoType);
    }

    public async Task<bool> DeletePhotoTypeAsync(int photoTypeId)
    {
        var photoType = await _photoTypeRepository.GetPhotoTypeByIdAsync(photoTypeId);
        if (photoType == null)
        {
            return false;
        }

        await _photoTypeRepository.DeleteAsync(photoType);
        await _photoTypeRepository.SaveChangesAsync();
        return true;
    }

    public async Task<bool> IsPhotoTypeNameExistsAsync(string photoTypeName, int? excludeId = null)
    {
        return await _photoTypeRepository.IsPhotoTypeNameExistsAsync(photoTypeName, excludeId);
    }

    private static PhotoTypeDto MapToPhotoTypeDto(Snapdi.Repositories.Models.PhotoType photoType)
    {
        return new PhotoTypeDto
        {
            PhotoTypeId = photoType.PhotoTypeId,
            PhotoTypeName = photoType.PhotoTypeName
        };
    }
}
