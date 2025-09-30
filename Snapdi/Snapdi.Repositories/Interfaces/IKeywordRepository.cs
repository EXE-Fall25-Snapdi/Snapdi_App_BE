using Snapdi.Repositories.Models;

namespace Snapdi.Repositories.Interfaces
{
    public interface IKeywordRepository : IBaseRepository<Keyword>
    {
        Task<IEnumerable<Keyword>> GetKeywordsWithBlogsAsync();
        Task<Keyword?> GetKeywordWithBlogsAsync(int keywordId);
        Task<IEnumerable<Keyword>> GetKeywordsByBlogAsync(int blogId);
        Task<Keyword?> GetByNameAsync(string keywordName);
        Task<Keyword> GetOrCreateKeywordAsync(string keywordName);
    }
}