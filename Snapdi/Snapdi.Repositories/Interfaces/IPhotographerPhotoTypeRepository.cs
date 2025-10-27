using Snapdi.Repositories.Models;

namespace Snapdi.Repositories.Interfaces
{
    public interface IPhotographerPhotoTypeRepository : IBaseRepository<PhotographerPhotoType>
    {
   Task<IEnumerable<PhotographerPhotoType>> GetByUserIdAsync(int userId);
        Task<PhotographerPhotoType?> GetByUserIdAndPhotoTypeIdAsync(int userId, int photoTypeId);
        Task<bool> ExistsByUserIdAndPhotoTypeIdAsync(int userId, int photoTypeId);
        Task DeleteByUserIdAsync(int userId);
   Task DeleteByUserIdAndPhotoTypeIdAsync(int userId, int photoTypeId);
        Task<IEnumerable<PhotographerPhotoType>> GetByPhotoTypeIdAsync(int photoTypeId);
        Task AddRangeAsync(IEnumerable<PhotographerPhotoType> photographerPhotoTypes);
    }
}
