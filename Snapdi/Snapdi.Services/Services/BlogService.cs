using Snapdi.Repositories.Interfaces;
using Snapdi.Repositories.Models;
using Snapdi.Services.DTOs;
using Snapdi.Services.Interfaces;

namespace Snapdi.Services.Services
{
    public class BlogService : IBlogService
    {
        private readonly IBlogRepository _blogRepository;
        private readonly IKeywordRepository _keywordRepository;

        public BlogService(IBlogRepository blogRepository, IKeywordRepository keywordRepository)
        {
            _blogRepository = blogRepository;
            _keywordRepository = keywordRepository;
        }

        public async Task<IEnumerable<BlogDto>> GetAllBlogsAsync()
        {
            var blogs = await _blogRepository.GetBlogsWithKeywordsAsync();
            return blogs.Select(MapToDto);
        }

        public async Task<PagedResult<BlogDto>> GetBlogsPagedAsync(PagedRequest pagedRequest)
        {
            var blogs = await _blogRepository.GetBlogsWithKeywordsPagedAsync(pagedRequest.PageNumber, pagedRequest.PageSize);
            var totalCount = await _blogRepository.GetTotalBlogsCountAsync();

            return new PagedResult<BlogDto>
            {
                Data = blogs.Select(MapToDto).ToList(),
                TotalRecords = totalCount,
                PageNumber = pagedRequest.PageNumber,
                PageSize = pagedRequest.PageSize
            };
        }

        public async Task<IEnumerable<BlogDto>> GetActiveBlogsAsync()
        {
            var blogs = await _blogRepository.GetActiveBlogsAsync();
            return blogs.Select(MapToDto);
        }

        public async Task<PagedResult<BlogDto>> GetActiveBlogsPagedAsync(PagedRequest pagedRequest)
        {
            var blogs = await _blogRepository.GetActiveBlogsPagedAsync(pagedRequest.PageNumber, pagedRequest.PageSize);
            var totalCount = await _blogRepository.GetActiveBlogsCountAsync();

            return new PagedResult<BlogDto>
            {
                Data = blogs.Select(MapToDto).ToList(),
                TotalRecords = totalCount,
                PageNumber = pagedRequest.PageNumber,
                PageSize = pagedRequest.PageSize
            };
        }

        public async Task<BlogDto?> GetBlogByIdAsync(int blogId)
        {
            var blog = await _blogRepository.GetBlogWithKeywordsAsync(blogId);
            return blog == null ? null : MapToDto(blog);
        }

        public async Task<IEnumerable<BlogDto>> GetBlogsByAuthorAsync(int authorId)
        {
            var blogs = await _blogRepository.GetBlogsByAuthorAsync(authorId);
            return blogs.Select(MapToDto);
        }

        public async Task<PagedResult<BlogDto>> GetBlogsByAuthorPagedAsync(int authorId, PagedRequest pagedRequest)
        {
            var blogs = await _blogRepository.GetBlogsByAuthorPagedAsync(authorId, pagedRequest.PageNumber, pagedRequest.PageSize);
            var totalCount = await _blogRepository.GetBlogsByAuthorCountAsync(authorId);

            return new PagedResult<BlogDto>
            {
                Data = blogs.Select(MapToDto).ToList(),
                TotalRecords = totalCount,
                PageNumber = pagedRequest.PageNumber,
                PageSize = pagedRequest.PageSize
            };
        }

        public async Task<IEnumerable<BlogDto>> GetBlogsByKeywordAsync(int keywordId)
        {
            var blogs = await _blogRepository.GetBlogsByKeywordAsync(keywordId);
            return blogs.Select(MapToDto);
        }

        public async Task<PagedResult<BlogDto>> GetBlogsByKeywordPagedAsync(int keywordId, PagedRequest pagedRequest)
        {
            var blogs = await _blogRepository.GetBlogsByKeywordPagedAsync(keywordId, pagedRequest.PageNumber, pagedRequest.PageSize);
            var totalCount = await _blogRepository.GetBlogsByKeywordCountAsync(keywordId);

            return new PagedResult<BlogDto>
            {
                Data = blogs.Select(MapToDto).ToList(),
                TotalRecords = totalCount,
                PageNumber = pagedRequest.PageNumber,
                PageSize = pagedRequest.PageSize
            };
        }

        public async Task<BlogDto> CreateBlogAsync(CreateBlogDto createBlogDto)
        {
            var blog = new Blog
            {
                Title = createBlogDto.Title,
                ThumbnailUrl = createBlogDto.ThumbnailUrl,
                Content = createBlogDto.Content,
                AuthorId = createBlogDto.AuthorId,
                CreateAt = DateTime.Now,
                IsActive = true
            };

            var createdBlog = await _blogRepository.AddAsync(blog);
            
            // Add keywords if provided
            if (createBlogDto.KeywordNames.Any() || createBlogDto.KeywordIds.Any())
            {
                await _blogRepository.SaveChangesAsync(); // Save blog first to get ID
                
                // Add keywords by name (create if not exist)
                foreach (var keywordName in createBlogDto.KeywordNames)
                {
                    var keyword = await _keywordRepository.GetOrCreateKeywordAsync(keywordName);
                    await _blogRepository.AddKeywordToBlogAsync(createdBlog.BlogId, keyword.KeywordId);
                }

                // Add keywords by ID
                foreach (var keywordId in createBlogDto.KeywordIds)
                {
                    await _blogRepository.AddKeywordToBlogAsync(createdBlog.BlogId, keywordId);
                }
            }

            await _blogRepository.SaveChangesAsync();

            // Reload blog with keywords
            var blogWithKeywords = await _blogRepository.GetBlogWithKeywordsAsync(createdBlog.BlogId);
            return MapToDto(blogWithKeywords!);
        }

        public async Task<BlogDto?> UpdateBlogAsync(int blogId, UpdateBlogDto updateBlogDto)
        {
            var blog = await _blogRepository.GetBlogWithKeywordsAsync(blogId);
            if (blog == null) return null;

            blog.Title = updateBlogDto.Title;
            blog.ThumbnailUrl = updateBlogDto.ThumbnailUrl;
            blog.Content = updateBlogDto.Content;
            blog.IsActive = updateBlogDto.IsActive;
            blog.UpdateAt = DateTime.Now;

            await _blogRepository.UpdateAsync(blog);

            // Update keywords if provided
            if (updateBlogDto.KeywordNames.Any() || updateBlogDto.KeywordIds.Any())
            {
                // Clear existing keywords
                var existingKeywords = blog.Keywords.ToList();
                foreach (var keyword in existingKeywords)
                {
                    await _blogRepository.RemoveKeywordFromBlogAsync(blogId, keyword.KeywordId);
                }

                // Add new keywords by name
                foreach (var keywordName in updateBlogDto.KeywordNames)
                {
                    var keyword = await _keywordRepository.GetOrCreateKeywordAsync(keywordName);
                    await _blogRepository.AddKeywordToBlogAsync(blogId, keyword.KeywordId);
                }

                // Add new keywords by ID
                foreach (var keywordId in updateBlogDto.KeywordIds)
                {
                    await _blogRepository.AddKeywordToBlogAsync(blogId, keywordId);
                }
            }

            await _blogRepository.SaveChangesAsync();

            // Reload blog with keywords
            var updatedBlog = await _blogRepository.GetBlogWithKeywordsAsync(blogId);
            return MapToDto(updatedBlog!);
        }

        public async Task<bool> DeleteBlogAsync(int blogId)
        {
            var blog = await _blogRepository.GetByIdAsync(blogId);
            if (blog == null) return false;

            await _blogRepository.DeleteAsync(blog);
            await _blogRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AddKeywordToBlogAsync(int blogId, int keywordId)
        {
            try
            {
                await _blogRepository.AddKeywordToBlogAsync(blogId, keywordId);
                await _blogRepository.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RemoveKeywordFromBlogAsync(int blogId, int keywordId)
        {
            try
            {
                await _blogRepository.RemoveKeywordFromBlogAsync(blogId, keywordId);
                await _blogRepository.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> AddKeywordsToBlogAsync(int blogId, List<int> keywordIds)
        {
            try
            {
                foreach (var keywordId in keywordIds)
                {
                    await _blogRepository.AddKeywordToBlogAsync(blogId, keywordId);
                }
                await _blogRepository.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> AddKeywordsToBlogAsync(int blogId, List<string> keywordNames)
        {
            try
            {
                foreach (var keywordName in keywordNames)
                {
                    var keyword = await _keywordRepository.GetOrCreateKeywordAsync(keywordName);
                    await _blogRepository.AddKeywordToBlogAsync(blogId, keyword.KeywordId);
                }
                await _blogRepository.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateBlogKeywordsAsync(int blogId, List<int> keywordIds)
        {
            try
            {
                var blog = await _blogRepository.GetBlogWithKeywordsAsync(blogId);
                if (blog == null) return false;

                // Remove all existing keywords
                var existingKeywords = blog.Keywords.ToList();
                foreach (var keyword in existingKeywords)
                {
                    await _blogRepository.RemoveKeywordFromBlogAsync(blogId, keyword.KeywordId);
                }

                // Add new keywords
                foreach (var keywordId in keywordIds)
                {
                    await _blogRepository.AddKeywordToBlogAsync(blogId, keywordId);
                }

                await _blogRepository.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateBlogKeywordsAsync(int blogId, List<string> keywordNames)
        {
            try
            {
                var blog = await _blogRepository.GetBlogWithKeywordsAsync(blogId);
                if (blog == null) return false;

                // Remove all existing keywords
                var existingKeywords = blog.Keywords.ToList();
                foreach (var keyword in existingKeywords)
                {
                    await _blogRepository.RemoveKeywordFromBlogAsync(blogId, keyword.KeywordId);
                }

                // Add new keywords by name
                foreach (var keywordName in keywordNames)
                {
                    var keyword = await _keywordRepository.GetOrCreateKeywordAsync(keywordName);
                    await _blogRepository.AddKeywordToBlogAsync(blogId, keyword.KeywordId);
                }

                await _blogRepository.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> BlogExistsAsync(int blogId)
        {
            return await _blogRepository.ExistsAsync(b => b.BlogId == blogId);
        }

        private static BlogDto MapToDto(Blog blog)
        {
            return new BlogDto
            {
                BlogId = blog.BlogId,
                AuthorId = blog.AuthorId,
                Title = blog.Title,
                ThumbnailUrl = blog.ThumbnailUrl,
                Content = blog.Content,
                CreateAt = blog.CreateAt,
                UpdateAt = blog.UpdateAt,
                IsActive = blog.IsActive,
                AuthorName = blog.Author?.Name,
                Keywords = blog.Keywords.Select(k => new KeywordDto
                {
                    KeywordId = k.KeywordId,
                    Keyword = k.Keyword1
                }).ToList()
            };
        }
    }
}