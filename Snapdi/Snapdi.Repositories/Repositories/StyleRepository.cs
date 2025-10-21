using Microsoft.EntityFrameworkCore;
using Snapdi.Repositories.Context;
using Snapdi.Repositories.Interfaces;
using Snapdi.Repositories.Models;

namespace Snapdi.Repositories.Repositories;

public class StyleRepository : BaseRepository<Style>, IStyleRepository
{
    public StyleRepository(SnapdiDbV2Context context) : base(context)
    {
    }

    public async Task<IEnumerable<Style>> GetAllStylesAsync()
    {
        return await _dbSet
            .OrderBy(s => s.StyleName)
            .ToListAsync();
    }

    public async Task<Style?> GetStyleByIdAsync(int styleId)
    {
        return await _dbSet
            .FirstOrDefaultAsync(s => s.StyleId == styleId);
    }

    public async Task<Style?> GetStyleByNameAsync(string styleName)
    {
        return await _dbSet
            .FirstOrDefaultAsync(s => s.StyleName == styleName);
    }

    public async Task<bool> IsStyleNameExistsAsync(string styleName, int? excludeId = null)
    {
        var query = _dbSet.Where(s => s.StyleName == styleName);
        
        if (excludeId.HasValue)
        {
            query = query.Where(s => s.StyleId != excludeId.Value);
        }

        return await query.AnyAsync();
    }
}
