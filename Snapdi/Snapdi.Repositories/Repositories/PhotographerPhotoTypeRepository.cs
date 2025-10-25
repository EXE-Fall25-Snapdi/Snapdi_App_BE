using Microsoft.EntityFrameworkCore;
using Snapdi.Repositories.Context;
using Snapdi.Repositories.Interfaces;
using Snapdi.Repositories.Models;

namespace Snapdi.Repositories.Repositories
{
    public class PhotographerPhotoTypeRepository : BaseRepository<PhotographerPhotoType>, IPhotographerPhotoTypeRepository
    {
        public PhotographerPhotoTypeRepository(SnapdiDbV2Context context) : base(context)
     {
 }

        public async Task<IEnumerable<PhotographerPhotoType>> GetByUserIdAsync(int userId)
        {
      return await _context.PhotographerPhotoTypes
     .Include(ppt => ppt.PhotoType)
   .Include(ppt => ppt.PhotographerProfile)
 .Where(ppt => ppt.UserId == userId)
     .ToListAsync();
        }

        public async Task<PhotographerPhotoType?> GetByUserIdAndPhotoTypeIdAsync(int userId, int photoTypeId)
      {
        return await _context.PhotographerPhotoTypes
    .Include(ppt => ppt.PhotoType)
      .Include(ppt => ppt.PhotographerProfile)
        .FirstOrDefaultAsync(ppt => ppt.UserId == userId && ppt.PhotoTypeId == photoTypeId);
        }

      public async Task<bool> ExistsByUserIdAndPhotoTypeIdAsync(int userId, int photoTypeId)
   {
  return await _context.PhotographerPhotoTypes
          .AnyAsync(ppt => ppt.UserId == userId && ppt.PhotoTypeId == photoTypeId);
      }

        public async Task DeleteByUserIdAsync(int userId)
        {
 var items = await _context.PhotographerPhotoTypes
              .Where(ppt => ppt.UserId == userId)
       .ToListAsync();
    
         _context.PhotographerPhotoTypes.RemoveRange(items);
        }

public async Task DeleteByUserIdAndPhotoTypeIdAsync(int userId, int photoTypeId)
        {
        var item = await _context.PhotographerPhotoTypes
        .FirstOrDefaultAsync(ppt => ppt.UserId == userId && ppt.PhotoTypeId == photoTypeId);
    
  if (item != null)
     {
         _context.PhotographerPhotoTypes.Remove(item);
            }
        }

      public async Task<IEnumerable<PhotographerPhotoType>> GetByPhotoTypeIdAsync(int photoTypeId)
        {
   return await _context.PhotographerPhotoTypes
                .Include(ppt => ppt.PhotoType)
          .Include(ppt => ppt.PhotographerProfile)
    .Where(ppt => ppt.PhotoTypeId == photoTypeId)
           .ToListAsync();
        }

        public async Task AddRangeAsync(IEnumerable<PhotographerPhotoType> photographerPhotoTypes)
        {
     await _context.PhotographerPhotoTypes.AddRangeAsync(photographerPhotoTypes);
        }
    }
}
