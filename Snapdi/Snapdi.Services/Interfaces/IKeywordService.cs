using Snapdi.Services.DTOs;

namespace Snapdi.Services.Interfaces
{
    public interface IKeywordService
    {
        Task<IEnumerable<KeywordDto>> GetAllKeywordsAsync();
        Task<KeywordDto?> GetKeywordByIdAsync(int keywordId);
        Task<KeywordWithBlogsDto?> GetKeywordWithBlogsAsync(int keywordId);
        Task<KeywordDto?> GetKeywordByNameAsync(string keywordName);
        Task<IEnumerable<KeywordDto>> GetKeywordsByBlogAsync(int blogId);
        Task<KeywordDto> CreateKeywordAsync(CreateKeywordDto createKeywordDto);
        Task<KeywordDto?> UpdateKeywordAsync(int keywordId, UpdateKeywordDto updateKeywordDto);
        Task<bool> DeleteKeywordAsync(int keywordId);
        Task<KeywordDto> GetOrCreateKeywordAsync(string keywordName);
        Task<bool> KeywordExistsAsync(int keywordId);
        Task<bool> KeywordExistsByNameAsync(string keywordName);
    }
}