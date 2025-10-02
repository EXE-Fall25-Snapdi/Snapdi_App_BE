using Microsoft.EntityFrameworkCore;
using Snapdi.Repositories.Context;
using Snapdi.Repositories.Interfaces;
using Snapdi.Repositories.Models;
using System.Linq.Expressions;

namespace Snapdi.Repositories.Repositories
{
    public class BlogRepository : BaseRepository<Blog>, IBlogRepository
    {
        public BlogRepository(SnapdiDbV2Context context) : base(context)
        {
        }

        public async Task<IEnumerable<Blog>> GetBlogsWithKeywordsAsync()
        {
            return await _dbSet
                .Include(b => b.Keywords)
                .Include(b => b.Author)
                .ToListAsync();
        }

        public async Task<IEnumerable<Blog>> GetBlogsWithKeywordsPagedAsync(int pageNumber, int pageSize)
        {
            return await _dbSet
                .Include(b => b.Keywords)
                .Include(b => b.Author)
                .OrderByDescending(b => b.CreateAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<Blog?> GetBlogWithKeywordsAsync(int blogId)
        {
            return await _dbSet
                .Include(b => b.Keywords)
                .Include(b => b.Author)
                .FirstOrDefaultAsync(b => b.BlogId == blogId);
        }

        public async Task<IEnumerable<Blog>> GetBlogsByKeywordAsync(int keywordId)
        {
            return await _dbSet
                .Include(b => b.Keywords)
                .Include(b => b.Author)
                .Where(b => b.Keywords.Any(k => k.KeywordId == keywordId))
                .ToListAsync();
        }

        public async Task<IEnumerable<Blog>> GetBlogsByKeywordPagedAsync(int keywordId, int pageNumber, int pageSize)
        {
            return await _dbSet
                .Include(b => b.Keywords)
                .Include(b => b.Author)
                .Where(b => b.Keywords.Any(k => k.KeywordId == keywordId))
                .OrderByDescending(b => b.CreateAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<Blog>> GetBlogsByAuthorAsync(int authorId)
        {
            return await _dbSet
                .Include(b => b.Keywords)
                .Include(b => b.Author)
                .Where(b => b.AuthorId == authorId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Blog>> GetBlogsByAuthorPagedAsync(int authorId, int pageNumber, int pageSize)
        {
            return await _dbSet
                .Include(b => b.Keywords)
                .Include(b => b.Author)
                .Where(b => b.AuthorId == authorId)
                .OrderByDescending(b => b.CreateAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<Blog>> GetActiveBlogsAsync()
        {
            return await _dbSet
                .Include(b => b.Keywords)
                .Include(b => b.Author)
                .Where(b => b.IsActive)
                .ToListAsync();
        }

        public async Task<IEnumerable<Blog>> GetActiveBlogsPagedAsync(int pageNumber, int pageSize)
        {
            return await _dbSet
                .Include(b => b.Keywords)
                .Include(b => b.Author)
                .Where(b => b.IsActive)
                .OrderByDescending(b => b.CreateAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task AddKeywordToBlogAsync(int blogId, int keywordId)
        {
            var blog = await _dbSet
                .Include(b => b.Keywords)
                .FirstOrDefaultAsync(b => b.BlogId == blogId);

            if (blog == null)
                throw new ArgumentException($"Blog with ID {blogId} not found");

            var keyword = await _context.Keywords.FindAsync(keywordId);
            if (keyword == null)
                throw new ArgumentException($"Keyword with ID {keywordId} not found");

            if (!blog.Keywords.Any(k => k.KeywordId == keywordId))
            {
                blog.Keywords.Add(keyword);
            }
        }

        public async Task RemoveKeywordFromBlogAsync(int blogId, int keywordId)
        {
            var blog = await _dbSet
                .Include(b => b.Keywords)
                .FirstOrDefaultAsync(b => b.BlogId == blogId);

            if (blog == null)
                throw new ArgumentException($"Blog with ID {blogId} not found");

            var keyword = blog.Keywords.FirstOrDefault(k => k.KeywordId == keywordId);
            if (keyword != null)
            {
                blog.Keywords.Remove(keyword);
            }
        }

        public async Task<bool> BlogHasKeywordAsync(int blogId, int keywordId)
        {
            return await _dbSet
                .AnyAsync(b => b.BlogId == blogId && b.Keywords.Any(k => k.KeywordId == keywordId));
        }

        public async Task<int> GetTotalBlogsCountAsync()
        {
            return await _dbSet.CountAsync();
        }

        public async Task<int> GetActiveBlogsCountAsync()
        {
            return await _dbSet.CountAsync(b => b.IsActive);
        }

        public async Task<int> GetBlogsByAuthorCountAsync(int authorId)
        {
            return await _dbSet.CountAsync(b => b.AuthorId == authorId);
        }

        public async Task<int> GetBlogsByKeywordCountAsync(int keywordId)
        {
            return await _dbSet.CountAsync(b => b.Keywords.Any(k => k.KeywordId == keywordId));
        }

        public override async Task<Blog> AddAsync(Blog entity)
        {
            entity.CreateAt = DateTime.Now;
            // Remove the hardcoded IsActive = true to allow setting from CreateBlogDto
            return await base.AddAsync(entity);
        }

        public override async Task UpdateAsync(Blog entity)
        {
            entity.UpdateAt = DateTime.Now;
            await base.UpdateAsync(entity);
        }
    }
}