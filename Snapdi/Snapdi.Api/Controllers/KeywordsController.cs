using Microsoft.AspNetCore.Mvc;
using Snapdi.Services.DTOs;
using Snapdi.Services.Interfaces;

namespace Snapdi.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class KeywordsController : ControllerBase
    {
        private readonly IKeywordService _keywordService;

        public KeywordsController(IKeywordService keywordService)
        {
            _keywordService = keywordService;
        }

        /// <summary>
        /// Get all keywords
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<KeywordDto>>> GetAllKeywords()
        {
            try
            {
                var keywords = await _keywordService.GetAllKeywordsAsync();
                return Ok(keywords);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get keyword by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<KeywordDto>> GetKeywordById(int id)
        {
            try
            {
                var keyword = await _keywordService.GetKeywordByIdAsync(id);
                if (keyword == null)
                {
                    return NotFound($"Keyword with ID {id} not found");
                }
                return Ok(keyword);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get keyword with associated blogs
        /// </summary>
        [HttpGet("{id}/blogs")]
        public async Task<ActionResult<KeywordWithBlogsDto>> GetKeywordWithBlogs(int id)
        {
            try
            {
                var keyword = await _keywordService.GetKeywordWithBlogsAsync(id);
                if (keyword == null)
                {
                    return NotFound($"Keyword with ID {id} not found");
                }
                return Ok(keyword);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get keyword by name
        /// </summary>
        [HttpGet("search")]
        public async Task<ActionResult<KeywordDto>> GetKeywordByName([FromQuery] string name)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    return BadRequest("Keyword name cannot be empty");
                }

                var keyword = await _keywordService.GetKeywordByNameAsync(name);
                if (keyword == null)
                {
                    return NotFound($"Keyword '{name}' not found");
                }
                return Ok(keyword);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get keywords by blog ID
        /// </summary>
        [HttpGet("blog/{blogId}")]
        public async Task<ActionResult<IEnumerable<KeywordDto>>> GetKeywordsByBlog(int blogId)
        {
            try
            {
                var keywords = await _keywordService.GetKeywordsByBlogAsync(blogId);
                return Ok(keywords);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Create a new keyword
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<KeywordDto>> CreateKeyword([FromBody] CreateKeywordDto createKeywordDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var keyword = await _keywordService.CreateKeywordAsync(createKeywordDto);
                return CreatedAtAction(nameof(GetKeywordById), new { id = keyword.KeywordId }, keyword);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Update an existing keyword
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<KeywordDto>> UpdateKeyword(int id, [FromBody] UpdateKeywordDto updateKeywordDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var keyword = await _keywordService.UpdateKeywordAsync(id, updateKeywordDto);
                if (keyword == null)
                {
                    return NotFound($"Keyword with ID {id} not found");
                }

                return Ok(keyword);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Delete a keyword
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteKeyword(int id)
        {
            try
            {
                var result = await _keywordService.DeleteKeywordAsync(id);
                if (!result)
                {
                    return NotFound($"Keyword with ID {id} not found");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get or create a keyword by name
        /// </summary>
        [HttpPost("get-or-create")]
        public async Task<ActionResult<KeywordDto>> GetOrCreateKeyword([FromBody] string keywordName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(keywordName))
                {
                    return BadRequest("Keyword name cannot be empty");
                }

                var keyword = await _keywordService.GetOrCreateKeywordAsync(keywordName);
                return Ok(keyword);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Check if keyword exists by ID
        /// </summary>
        [HttpGet("{id}/exists")]
        public async Task<ActionResult<bool>> KeywordExists(int id)
        {
            try
            {
                var exists = await _keywordService.KeywordExistsAsync(id);
                return Ok(exists);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Check if keyword exists by name
        /// </summary>
        [HttpGet("exists")]
        public async Task<ActionResult<bool>> KeywordExistsByName([FromQuery] string name)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    return BadRequest("Keyword name cannot be empty");
                }

                var exists = await _keywordService.KeywordExistsByNameAsync(name);
                return Ok(exists);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}