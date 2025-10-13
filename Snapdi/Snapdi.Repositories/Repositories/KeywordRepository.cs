using Microsoft.EntityFrameworkCore;
using Snapdi.Repositories.Context;
using Snapdi.Repositories.Interfaces;
using Snapdi.Repositories.Models;

namespace Snapdi.Repositories.Repositories
{
    public class KeywordRepository : BaseRepository<Keyword>, IKeywordRepository
    {
        public KeywordRepository(SnapdiDbV2Context context) : base(context)
        {
        }

        public async Task<IEnumerable<Keyword>> GetKeywordsWithBlogsAsync()
        {
            return await _dbSet
                .Include(k => k.Blogs)
                .ToListAsync();
        }

        public async Task<Keyword?> GetKeywordWithBlogsAsync(int keywordId)
        {
            return await _dbSet
                .Include(k => k.Blogs)
                .FirstOrDefaultAsync(k => k.KeywordId == keywordId);
        }

        public async Task<IEnumerable<Keyword>> GetKeywordsByBlogAsync(int blogId)
        {
            return await _dbSet
                .Include(k => k.Blogs)
                .Where(k => k.Blogs.Any(b => b.BlogId == blogId))
                .ToListAsync();
        }

        public async Task<Keyword?> GetByNameAsync(string keywordName)
        {
            return await _dbSet
                .FirstOrDefaultAsync(k => k.Keyword1.ToLower() == keywordName.ToLower());
        }

        public async Task<Keyword> GetOrCreateKeywordAsync(string keywordName)
        {
            var existingKeyword = await GetByNameAsync(keywordName);
            if (existingKeyword != null)
            {
                return existingKeyword;
            }

            var newKeyword = new Keyword
            {
                Keyword1 = keywordName
            };

            var result = await AddAsync(newKeyword);
            await SaveChangesAsync();
            return result;
        }
    }
}