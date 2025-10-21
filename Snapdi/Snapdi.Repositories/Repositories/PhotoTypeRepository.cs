using Microsoft.EntityFrameworkCore;
using Snapdi.Repositories.Context;
using Snapdi.Repositories.Interfaces;
using Snapdi.Repositories.Models;

namespace Snapdi.Repositories.Repositories;

public class PhotoTypeRepository : BaseRepository<PhotoType>, IPhotoTypeRepository
{
    public PhotoTypeRepository(SnapdiDbV2Context context) : base(context)
    {
    }

    public async Task<IEnumerable<PhotoType>> GetAllPhotoTypesAsync()
    {
        return await _dbSet
            .OrderBy(pt => pt.PhotoTypeName)
            .ToListAsync();
    }

    public async Task<PhotoType?> GetPhotoTypeByIdAsync(int photoTypeId)
    {
        return await _dbSet
            .FirstOrDefaultAsync(pt => pt.PhotoTypeId == photoTypeId);
    }

    public async Task<PhotoType?> GetPhotoTypeByNameAsync(string photoTypeName)
    {
        return await _dbSet
            .FirstOrDefaultAsync(pt => pt.PhotoTypeName == photoTypeName);
    }

    public async Task<bool> IsPhotoTypeNameExistsAsync(string photoTypeName, int? excludeId = null)
    {
        var query = _dbSet.Where(pt => pt.PhotoTypeName == photoTypeName);
        
        if (excludeId.HasValue)
        {
            query = query.Where(pt => pt.PhotoTypeId != excludeId.Value);
        }

        return await query.AnyAsync();
    }
}
