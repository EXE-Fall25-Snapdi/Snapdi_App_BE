using Microsoft.AspNetCore.Mvc;
using Snapdi.Services.DTOs;
using Snapdi.Services.Interfaces;

namespace Snapdi.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BlogsController : ControllerBase
    {
        private readonly IBlogService _blogService;

        public BlogsController(IBlogService blogService)
        {
            _blogService = blogService;
        }

        /// <summary>
        /// Get all blogs
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BlogDto>>> GetAllBlogs()
        {
            try
            {
                var blogs = await _blogService.GetAllBlogsAsync();
                return Ok(blogs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get blogs with paging
        /// </summary>
        [HttpGet("paged")]
        public async Task<ActionResult<PagedResult<BlogDto>>> GetBlogsPaged([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 10;

                var pagedRequest = new PagedRequest { PageNumber = pageNumber, PageSize = pageSize };
                var result = await _blogService.GetBlogsPagedAsync(pagedRequest);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get active blogs only
        /// </summary>
        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<BlogDto>>> GetActiveBlogs()
        {
            try
            {
                var blogs = await _blogService.GetActiveBlogsAsync();
                return Ok(blogs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get active blogs with paging
        /// </summary>
        [HttpGet("active/paged")]
        public async Task<ActionResult<PagedResult<BlogDto>>> GetActiveBlogsPaged([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 10;

                var pagedRequest = new PagedRequest { PageNumber = pageNumber, PageSize = pageSize };
                var result = await _blogService.GetActiveBlogsPagedAsync(pagedRequest);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get blog by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<BlogDto>> GetBlogById(int id)
        {
        try
        {
            var blog = await _blogService.GetBlogByIdAsync(id);
            if (blog == null)
            {
                return NotFound($"Blog with ID {id} not found");
            }
            return Ok(blog);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
        }

        /// <summary>
        /// Get blogs by author ID
        /// </summary>
        [HttpGet("author/{authorId}")]
        public async Task<ActionResult<IEnumerable<BlogDto>>> GetBlogsByAuthor(int authorId)
        {
            try
            {
                var blogs = await _blogService.GetBlogsByAuthorAsync(authorId);
                return Ok(blogs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get blogs by author ID with paging
        /// </summary>
        [HttpGet("author/{authorId}/paged")]
        public async Task<ActionResult<PagedResult<BlogDto>>> GetBlogsByAuthorPaged(int authorId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 10;

                var pagedRequest = new PagedRequest { PageNumber = pageNumber, PageSize = pageSize };
                var result = await _blogService.GetBlogsByAuthorPagedAsync(authorId, pagedRequest);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get blogs by keyword ID
        /// </summary>
        [HttpGet("keyword/{keywordId}")]
        public async Task<ActionResult<IEnumerable<BlogDto>>> GetBlogsByKeyword(int keywordId)
        {
            try
            {
                var blogs = await _blogService.GetBlogsByKeywordAsync(keywordId);
                return Ok(blogs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get blogs by keyword ID with paging
        /// </summary>
        [HttpGet("keyword/{keywordId}/paged")]
        public async Task<ActionResult<PagedResult<BlogDto>>> GetBlogsByKeywordPaged(int keywordId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 10;

                var pagedRequest = new PagedRequest { PageNumber = pageNumber, PageSize = pageSize };
                var result = await _blogService.GetBlogsByKeywordPagedAsync(keywordId, pagedRequest);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Create a new blog
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<BlogDto>> CreateBlog([FromBody] CreateBlogDto createBlogDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var blog = await _blogService.CreateBlogAsync(createBlogDto);
                return CreatedAtAction(nameof(GetBlogById), new { id = blog.BlogId }, blog);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Update an existing blog
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<BlogDto>> UpdateBlog(int id, [FromBody] UpdateBlogDto updateBlogDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var blog = await _blogService.UpdateBlogAsync(id, updateBlogDto);
                if (blog == null)
                {
                    return NotFound($"Blog with ID {id} not found");
                }

                return Ok(blog);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Delete a blog
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteBlog(int id)
        {
            try
            {
                var result = await _blogService.DeleteBlogAsync(id);
                if (!result)
                {
                    return NotFound($"Blog with ID {id} not found");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Add a keyword to a blog
        /// </summary>
        [HttpPost("{blogId}/keywords/{keywordId}")]
        public async Task<ActionResult> AddKeywordToBlog(int blogId, int keywordId)
        {
            try
            {
                var result = await _blogService.AddKeywordToBlogAsync(blogId, keywordId);
                if (!result)
                {
                    return BadRequest("Failed to add keyword to blog. Check if blog and keyword exist.");
                }

                return Ok("Keyword added to blog successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Remove a keyword from a blog
        /// </summary>
        [HttpDelete("{blogId}/keywords/{keywordId}")]
        public async Task<ActionResult> RemoveKeywordFromBlog(int blogId, int keywordId)
        {
            try
            {
                var result = await _blogService.RemoveKeywordFromBlogAsync(blogId, keywordId);
                if (!result)
                {
                    return BadRequest("Failed to remove keyword from blog");
                }

                return Ok("Keyword removed from blog successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Add multiple keywords to a blog by keyword IDs
        /// </summary>
        [HttpPost("{blogId}/keywords")]
        public async Task<ActionResult> AddKeywordsToBlog(int blogId, [FromBody] List<int> keywordIds)
        {
            try
            {
                if (keywordIds == null || !keywordIds.Any())
                {
                    return BadRequest("Keyword IDs list cannot be empty");
                }

                var result = await _blogService.AddKeywordsToBlogAsync(blogId, keywordIds);
                if (!result)
                {
                    return BadRequest("Failed to add keywords to blog");
                }

                return Ok("Keywords added to blog successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Add multiple keywords to a blog by keyword names
        /// </summary>
        [HttpPost("{blogId}/keywords/by-names")]
        public async Task<ActionResult> AddKeywordsToBlogByNames(int blogId, [FromBody] List<string> keywordNames)
        {
            try
            {
                if (keywordNames == null || !keywordNames.Any())
                {
                    return BadRequest("Keyword names list cannot be empty");
                }

                var result = await _blogService.AddKeywordsToBlogAsync(blogId, keywordNames);
                if (!result)
                {
                    return BadRequest("Failed to add keywords to blog");
                }

                return Ok("Keywords added to blog successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Update all keywords for a blog by IDs
        /// </summary>
        [HttpPut("{blogId}/keywords")]
        public async Task<ActionResult> UpdateBlogKeywords(int blogId, [FromBody] List<int> keywordIds)
        {
            try
            {
                if (keywordIds == null)
                {
                    return BadRequest("Keyword IDs list cannot be null");
                }

                var result = await _blogService.UpdateBlogKeywordsAsync(blogId, keywordIds);
                if (!result)
                {
                    return BadRequest("Failed to update blog keywords");
                }

                return Ok("Blog keywords updated successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Update all keywords for a blog by names
        /// </summary>
        [HttpPut("{blogId}/keywords/by-names")]
        public async Task<ActionResult> UpdateBlogKeywordsByNames(int blogId, [FromBody] List<string> keywordNames)
        {
            try
            {
                if (keywordNames == null)
                {
                    return BadRequest("Keyword names list cannot be null");
                }

                var result = await _blogService.UpdateBlogKeywordsAsync(blogId, keywordNames);
                if (!result)
                {
                    return BadRequest("Failed to update blog keywords");
                }

                return Ok("Blog keywords updated successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}