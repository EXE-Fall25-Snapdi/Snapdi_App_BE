using Snapdi.Repositories.Models;

namespace Snapdi.Repositories.Interfaces
{
    public interface IBlogRepository : IBaseRepository<Blog>
    {
        Task<IEnumerable<Blog>> GetBlogsWithKeywordsAsync();
        Task<IEnumerable<Blog>> GetBlogsWithKeywordsPagedAsync(int pageNumber, int pageSize);
        Task<Blog?> GetBlogWithKeywordsAsync(int blogId);
        Task<IEnumerable<Blog>> GetBlogsByKeywordAsync(int keywordId);
        Task<IEnumerable<Blog>> GetBlogsByKeywordPagedAsync(int keywordId, int pageNumber, int pageSize);
        Task<IEnumerable<Blog>> GetBlogsByAuthorAsync(int authorId);
        Task<IEnumerable<Blog>> GetBlogsByAuthorPagedAsync(int authorId, int pageNumber, int pageSize);
        Task<IEnumerable<Blog>> GetActiveBlogsAsync();
        Task<IEnumerable<Blog>> GetActiveBlogsPagedAsync(int pageNumber, int pageSize);
        Task AddKeywordToBlogAsync(int blogId, int keywordId);
        Task RemoveKeywordFromBlogAsync(int blogId, int keywordId);
        Task<bool> BlogHasKeywordAsync(int blogId, int keywordId);
        Task<int> GetTotalBlogsCountAsync();
        Task<int> GetActiveBlogsCountAsync();
        Task<int> GetBlogsByAuthorCountAsync(int authorId);
        Task<int> GetBlogsByKeywordCountAsync(int keywordId);
    }
}