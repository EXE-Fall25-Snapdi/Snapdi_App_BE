using Snapdi.Repositories.Models;

namespace Snapdi.Repositories.Interfaces;

public interface IPhotoTypeRepository : IBaseRepository<PhotoType>
{
    Task<IEnumerable<PhotoType>> GetAllPhotoTypesAsync();
    Task<PhotoType?> GetPhotoTypeByIdAsync(int photoTypeId);
    Task<PhotoType?> GetPhotoTypeByNameAsync(string photoTypeName);
    Task<bool> IsPhotoTypeNameExistsAsync(string photoTypeName, int? excludeId = null);
}
