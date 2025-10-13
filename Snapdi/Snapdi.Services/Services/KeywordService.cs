using Snapdi.Repositories.Interfaces;
using Snapdi.Repositories.Models;
using Snapdi.Services.DTOs;
using Snapdi.Services.Interfaces;

namespace Snapdi.Services.Services
{
    public class KeywordService : IKeywordService
    {
        private readonly IKeywordRepository _keywordRepository;

        public KeywordService(IKeywordRepository keywordRepository)
        {
            _keywordRepository = keywordRepository;
        }

        public async Task<IEnumerable<KeywordDto>> GetAllKeywordsAsync()
        {
            var keywords = await _keywordRepository.GetKeywordsWithBlogsAsync();
            return keywords.Select(MapToDto);
        }

        public async Task<KeywordDto?> GetKeywordByIdAsync(int keywordId)
        {
            var keyword = await _keywordRepository.GetByIdAsync(keywordId);
            return keyword == null ? null : MapToDto(keyword);
        }

        public async Task<KeywordWithBlogsDto?> GetKeywordWithBlogsAsync(int keywordId)
        {
            var keyword = await _keywordRepository.GetKeywordWithBlogsAsync(keywordId);
            return keyword == null ? null : MapToDetailDto(keyword);
        }

        public async Task<KeywordDto?> GetKeywordByNameAsync(string keywordName)
        {
            var keyword = await _keywordRepository.GetByNameAsync(keywordName);
            return keyword == null ? null : MapToDto(keyword);
        }

        public async Task<IEnumerable<KeywordDto>> GetKeywordsByBlogAsync(int blogId)
        {
            var keywords = await _keywordRepository.GetKeywordsByBlogAsync(blogId);
            return keywords.Select(MapToDto);
        }

        public async Task<KeywordDto> CreateKeywordAsync(CreateKeywordDto createKeywordDto)
        {
            // Check if keyword already exists
            var existingKeyword = await _keywordRepository.GetByNameAsync(createKeywordDto.Keyword);
            if (existingKeyword != null)
            {
                throw new InvalidOperationException($"Keyword '{createKeywordDto.Keyword}' already exists");
            }

            var keyword = new Keyword
            {
                Keyword1 = createKeywordDto.Keyword
            };

            var createdKeyword = await _keywordRepository.AddAsync(keyword);
            await _keywordRepository.SaveChangesAsync();

            return MapToDto(createdKeyword);
        }

        public async Task<KeywordDto?> UpdateKeywordAsync(int keywordId, UpdateKeywordDto updateKeywordDto)
        {
            var keyword = await _keywordRepository.GetByIdAsync(keywordId);
            if (keyword == null) return null;

            // Check if another keyword with the same name exists
            var existingKeyword = await _keywordRepository.GetByNameAsync(updateKeywordDto.Keyword);
            if (existingKeyword != null && existingKeyword.KeywordId != keywordId)
            {
                throw new InvalidOperationException($"Keyword '{updateKeywordDto.Keyword}' already exists");
            }

            keyword.Keyword1 = updateKeywordDto.Keyword;

            await _keywordRepository.UpdateAsync(keyword);
            await _keywordRepository.SaveChangesAsync();

            return MapToDto(keyword);
        }

        public async Task<bool> DeleteKeywordAsync(int keywordId)
        {
            var keyword = await _keywordRepository.GetByIdAsync(keywordId);
            if (keyword == null) return false;

            await _keywordRepository.DeleteAsync(keyword);
            await _keywordRepository.SaveChangesAsync();
            return true;
        }

        public async Task<KeywordDto> GetOrCreateKeywordAsync(string keywordName)
        {
            var existingKeyword = await _keywordRepository.GetByNameAsync(keywordName);
            if (existingKeyword != null)
            {
                return MapToDto(existingKeyword);
            }

            var keyword = new Keyword
            {
                Keyword1 = keywordName
            };

            var createdKeyword = await _keywordRepository.AddAsync(keyword);
            await _keywordRepository.SaveChangesAsync();

            return MapToDto(createdKeyword);
        }

        public async Task<bool> KeywordExistsAsync(int keywordId)
        {
            return await _keywordRepository.ExistsAsync(k => k.KeywordId == keywordId);
        }

        public async Task<bool> KeywordExistsByNameAsync(string keywordName)
        {
            return await _keywordRepository.ExistsAsync(k => k.Keyword1.ToLower() == keywordName.ToLower());
        }

        private static KeywordDto MapToDto(Keyword keyword)
        {
            return new KeywordDto
            {
                KeywordId = keyword.KeywordId,
                Keyword = keyword.Keyword1,
                BlogCount = keyword.Blogs?.Count ?? 0
            };
        }

        private static KeywordWithBlogsDto MapToDetailDto(Keyword keyword)
        {
            return new KeywordWithBlogsDto
            {
                KeywordId = keyword.KeywordId,
                Keyword = keyword.Keyword1,
                BlogCount = keyword.Blogs?.Count ?? 0,
                Blogs = keyword.Blogs?.Select(b => new BlogSummaryDto
                {
                    BlogId = b.BlogId,
                    Title = b.Title,
                    ThumbnailUrl = b.ThumbnailUrl,
                    CreateAt = b.CreateAt,
                    IsActive = b.IsActive,
                    AuthorName = b.Author?.Name
                }).ToList() ?? new List<BlogSummaryDto>()
            };
        }
    }
}