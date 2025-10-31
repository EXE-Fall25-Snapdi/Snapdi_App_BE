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

        public async Task<IEnumerable<Blog>> SearchBlogsAsync(BlogSearchParameters searchParameters)
        {
            var query = _dbSet
                .Include(b => b.Keywords)
                .Include(b => b.Author)
                .AsQueryable();

            // Apply search term filter (search in title and content)
            if (!string.IsNullOrWhiteSpace(searchParameters.SearchTerm))
            {
                var searchTerm = searchParameters.SearchTerm.ToLower();
                query = query.Where(b => b.Title.ToLower().Contains(searchTerm) || 
                                       b.Content.ToLower().Contains(searchTerm));
            }

            // Apply author filter
            if (searchParameters.AuthorId.HasValue)
            {
                query = query.Where(b => b.AuthorId == searchParameters.AuthorId.Value);
            }

            // Apply active status filter
            if (searchParameters.IsActive.HasValue)
            {
                query = query.Where(b => b.IsActive == searchParameters.IsActive.Value);
            }

            // Apply date range filters
            if (searchParameters.DateFrom.HasValue)
            {
                query = query.Where(b => b.CreateAt >= searchParameters.DateFrom.Value);
            }

            if (searchParameters.DateTo.HasValue)
            {
                query = query.Where(b => b.CreateAt <= searchParameters.DateTo.Value);
            }

            // Apply keyword filters
            if (searchParameters.KeywordIds != null && searchParameters.KeywordIds.Any())
            {
                query = query.Where(b => b.Keywords.Any(k => searchParameters.KeywordIds.Contains(k.KeywordId)));
            }

            if (searchParameters.Keywords != null && searchParameters.Keywords.Any())
            {
                var keywordNames = searchParameters.Keywords.Select(k => k.ToLower()).ToList();
                query = query.Where(b => b.Keywords.Any(k => keywordNames.Contains(k.Keyword1.ToLower())));
            }

            // Apply pagination and ordering
            return await query
                .OrderByDescending(b => b.CreateAt)
                .Skip((searchParameters.PageNumber - 1) * searchParameters.PageSize)
                .Take(searchParameters.PageSize)
                .ToListAsync();
        }

        public async Task<int> GetSearchBlogsCountAsync(BlogSearchParameters searchParameters)
        {
            var query = _dbSet.AsQueryable();

            // Apply same filters as SearchBlogsAsync but without includes for counting
            if (!string.IsNullOrWhiteSpace(searchParameters.SearchTerm))
            {
                var searchTerm = searchParameters.SearchTerm.ToLower();
                query = query.Where(b => b.Title.ToLower().Contains(searchTerm) || 
                                       b.Content.ToLower().Contains(searchTerm));
            }

            if (searchParameters.AuthorId.HasValue)
            {
                query = query.Where(b => b.AuthorId == searchParameters.AuthorId.Value);
            }

            if (searchParameters.IsActive.HasValue)
            {
                query = query.Where(b => b.IsActive == searchParameters.IsActive.Value);
            }

            if (searchParameters.DateFrom.HasValue)
            {
                query = query.Where(b => b.CreateAt >= searchParameters.DateFrom.Value);
            }

            if (searchParameters.DateTo.HasValue)
            {
                query = query.Where(b => b.CreateAt <= searchParameters.DateTo.Value);
            }

            if (searchParameters.KeywordIds != null && searchParameters.KeywordIds.Any())
            {
                query = query.Where(b => b.Keywords.Any(k => searchParameters.KeywordIds.Contains(k.KeywordId)));
            }

            if (searchParameters.Keywords != null && searchParameters.Keywords.Any())
            {
                var keywordNames = searchParameters.Keywords.Select(k => k.ToLower()).ToList();
                query = query.Where(b => b.Keywords.Any(k => keywordNames.Contains(k.Keyword1.ToLower())));
            }

            return await query.CountAsync();
        }

        public override async Task<Blog> AddAsync(Blog entity)
        {
            // Ensure DateTime is explicitly UTC for PostgreSQL
            entity.CreateAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
            // Remove the hardcoded IsActive = true to allow setting from CreateBlogDto
            return await base.AddAsync(entity);
        }

        public override async Task UpdateAsync(Blog entity)
        {
            // Ensure DateTime is explicitly UTC for PostgreSQL
            entity.UpdateAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
            
            // Ensure CreateAt is also UTC if it was loaded from database
            if (entity.CreateAt.Kind != DateTimeKind.Utc)
            {
                entity.CreateAt = DateTime.SpecifyKind(entity.CreateAt, DateTimeKind.Utc);
            }
            
            await base.UpdateAsync(entity);
        }
    }
}