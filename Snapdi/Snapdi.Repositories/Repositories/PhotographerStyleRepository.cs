using Microsoft.EntityFrameworkCore;
using Snapdi.Repositories.Context;
using Snapdi.Repositories.Interfaces;
using Snapdi.Repositories.Models;

namespace Snapdi.Repositories.Repositories;

public class PhotographerStyleRepository : BaseRepository<PhotographerStyle>, IPhotographerStyleRepository
{
    public PhotographerStyleRepository(SnapdiDbV2Context context) : base(context)
    {
    }

    public async Task<IEnumerable<PhotographerStyle>> GetPhotographerStylesAsync(int userId)
    {
        return await _dbSet
            .Include(ps => ps.Style)
            .Include(ps => ps.PhotographerProfile)
            .ThenInclude(pp => pp.User)
            .Where(ps => ps.UserId == userId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Style>> GetStylesByPhotographerAsync(int userId)
    {
        return await _dbSet
            .Include(ps => ps.Style)
            .Where(ps => ps.UserId == userId)
            .Select(ps => ps.Style)
            .ToListAsync();
    }

    public async Task<IEnumerable<PhotographerStyle>> GetPhotographersByStyleAsync(int styleId)
    {
        return await _dbSet
            .Include(ps => ps.PhotographerProfile)
            .ThenInclude(pp => pp.User)
            .Include(ps => ps.Style)
            .Where(ps => ps.StyleId == styleId)
            .ToListAsync();
    }

    public async Task<bool> IsPhotographerStyleExistsAsync(int userId, int styleId)
    {
        return await _dbSet
            .AnyAsync(ps => ps.UserId == userId && ps.StyleId == styleId);
    }

    public async Task<PhotographerStyle?> GetPhotographerStyleAsync(int userId, int styleId)
    {
        return await _dbSet
            .Include(ps => ps.Style)
            .Include(ps => ps.PhotographerProfile)
            .ThenInclude(pp => pp.User)
            .FirstOrDefaultAsync(ps => ps.UserId == userId && ps.StyleId == styleId);
    }

    public async Task<bool> AddPhotographerStyleAsync(int userId, int styleId)
    {
        if (await IsPhotographerStyleExistsAsync(userId, styleId))
        {
            return false; // Already exists
        }

        var photographerStyle = new PhotographerStyle
        {
            UserId = userId,
            StyleId = styleId
        };

        await _dbSet.AddAsync(photographerStyle);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AddMultiplePhotographerStylesAsync(int userId, IEnumerable<int> styleIds)
    {
        var styleIdList = styleIds.ToList();
        if (!styleIdList.Any())
        {
            return true; // Nothing to add
        }

        // Get existing style IDs for this photographer
        var existingStyleIds = (await _dbSet
            .Where(ps => ps.UserId == userId)
            .Select(ps => ps.StyleId)
            .ToListAsync()).ToHashSet();

        // Filter out styles that already exist
        var newStyleIds = styleIdList.Where(styleId => !existingStyleIds.Contains(styleId)).ToList();

        if (!newStyleIds.Any())
        {
            return true; // All styles already exist
        }

        try
        {
            var newPhotographerStyles = newStyleIds.Select(styleId => new PhotographerStyle
            {
                UserId = userId,
                StyleId = styleId
            });

            await _dbSet.AddRangeAsync(newPhotographerStyles);
            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> RemovePhotographerStyleAsync(int userId, int styleId)
    {
        var photographerStyle = await _dbSet
            .FirstOrDefaultAsync(ps => ps.UserId == userId && ps.StyleId == styleId);

        if (photographerStyle == null)
        {
            return false;
        }

        _dbSet.Remove(photographerStyle);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdatePhotographerStylesAsync(int userId, IEnumerable<int> styleIds)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Remove existing styles for this photographer
            var existingStyles = await _dbSet
                .Where(ps => ps.UserId == userId)
                .ToListAsync();

            _dbSet.RemoveRange(existingStyles);

            // Add new styles
            var newStyles = styleIds.Select(styleId => new PhotographerStyle
            {
                UserId = userId,
                StyleId = styleId
            });

            await _dbSet.AddRangeAsync(newStyles);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            return false;
        }
    }

    public async Task<bool> UpdatePhotographerStylesDifferentialAsync(int userId, IEnumerable<int> newStyleIds)
    {
        var newStyleIdList = newStyleIds.ToList();
        
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Get current style IDs for this photographer
            var currentStyleIds = await _dbSet
                .Where(ps => ps.UserId == userId)
                .Select(ps => ps.StyleId)
                .ToListAsync();

            // Determine which styles to add and remove
            var stylesToAdd = newStyleIdList.Except(currentStyleIds).ToList();
            var stylesToRemove = currentStyleIds.Except(newStyleIdList).ToList();

            // Remove styles that are no longer selected
            if (stylesToRemove.Any())
            {
                var stylesToRemoveEntities = await _dbSet
                    .Where(ps => ps.UserId == userId && stylesToRemove.Contains(ps.StyleId))
                    .ToListAsync();

                _dbSet.RemoveRange(stylesToRemoveEntities);
            }

            // Add new styles
            if (stylesToAdd.Any())
            {
                var newStyles = stylesToAdd.Select(styleId => new PhotographerStyle
                {
                    UserId = userId,
                    StyleId = styleId
                });

                await _dbSet.AddRangeAsync(newStyles);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            return false;
        }
    }
}
