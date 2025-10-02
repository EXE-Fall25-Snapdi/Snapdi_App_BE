using Snapdi.Services.DTOs;

namespace Snapdi.Services.Interfaces
{
    public interface IBlogService
    {
        Task<IEnumerable<BlogDto>> GetAllBlogsAsync();
        Task<PagedResult<BlogDto>> GetBlogsPagedAsync(PagedRequest pagedRequest);
        Task<IEnumerable<BlogDto>> GetActiveBlogsAsync();
        Task<PagedResult<BlogDto>> GetActiveBlogsPagedAsync(PagedRequest pagedRequest);
        Task<BlogDto?> GetBlogByIdAsync(int blogId);
        Task<IEnumerable<BlogDto>> GetBlogsByAuthorAsync(int authorId);
        Task<PagedResult<BlogDto>> GetBlogsByAuthorPagedAsync(int authorId, PagedRequest pagedRequest);
        Task<IEnumerable<BlogDto>> GetBlogsByKeywordAsync(int keywordId);
        Task<PagedResult<BlogDto>> GetBlogsByKeywordPagedAsync(int keywordId, PagedRequest pagedRequest);
        Task<BlogDto> CreateBlogAsync(CreateBlogDto createBlogDto);
        Task<BlogDto?> UpdateBlogAsync(int blogId, UpdateBlogDto updateBlogDto);
        Task<bool> DeleteBlogAsync(int blogId);
        Task<bool> AddKeywordToBlogAsync(int blogId, int keywordId);
        Task<bool> RemoveKeywordFromBlogAsync(int blogId, int keywordId);
        Task<bool> AddKeywordsToBlogAsync(int blogId, List<int> keywordIds);
        Task<bool> AddKeywordsToBlogAsync(int blogId, List<string> keywordNames);
        Task<bool> UpdateBlogKeywordsAsync(int blogId, List<int> keywordIds);
        Task<bool> UpdateBlogKeywordsAsync(int blogId, List<string> keywordNames);
        Task<bool> BlogExistsAsync(int blogId);
    }
}