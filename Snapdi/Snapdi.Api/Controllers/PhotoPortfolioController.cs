using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Snapdi.Services.DTOs;
using Snapdi.Services.Interfaces;
using System.Security.Claims;

namespace Snapdi.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PhotoPortfolioController : ControllerBase
    {
        private readonly IPhotoPortfolioService _photoPortfolioService;
        private readonly IUserService _userService;

        public PhotoPortfolioController(IPhotoPortfolioService photoPortfolioService, IUserService userService)
        {
            _photoPortfolioService = photoPortfolioService;
            _userService = userService;
        }

        /// <summary>
        /// Get photo portfolios by user ID
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>List of photo portfolios for the user</returns>
        [HttpGet("user/{userId:int}")]
        public async Task<ActionResult<IEnumerable<PhotoPortfolioDto>>> GetPhotoPortfoliosByUserId(int userId)
        {
            var portfolios = await _userService.GetPhotoPortfoliosByUserIdAsync(userId);
            return Ok(portfolios);
        }

        /// <summary>
        /// Get photo portfolios for the current authenticated user
        /// </summary>
        /// <returns>List of photo portfolios for the current user</returns>
        [HttpGet("my-portfolios")]
        public async Task<ActionResult<IEnumerable<PhotoPortfolioDto>>> GetMyPhotoPortfolios()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized("Invalid user token");
            }

            var portfolios = await _photoPortfolioService.GetPhotoPortfoliosByUserIdAsync(userId);
            return Ok(portfolios);
        }

        /// <summary>
        /// Get a specific photo portfolio by ID
        /// </summary>
        /// <param name="id">The photo portfolio ID</param>
        /// <returns>The photo portfolio if found</returns>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<PhotoPortfolioDto>> GetPhotoPortfolioById(int id)
        {
            var portfolio = await _photoPortfolioService.GetPhotoPortfolioByIdAsync(id);
            if (portfolio == null)
            {
                return NotFound($"Photo portfolio with ID {id} not found");
            }

            return Ok(portfolio);
        }

        /// <summary>
        /// Create a new photo portfolio for the current user
        /// </summary>
        /// <param name="createDto">The photo portfolio creation data</param>
        /// <returns>The created photo portfolio</returns>
        [HttpPost]
        public async Task<ActionResult<PhotoPortfolioDto>> CreatePhotoPortfolio([FromBody] CreatePhotoPortfolioDto createDto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized("Invalid user token");
            }

            try
            {
                var portfolio = await _photoPortfolioService.CreatePhotoPortfolioAsync(userId, createDto);
                return CreatedAtAction(nameof(GetPhotoPortfolioById), new { id = portfolio.PhotoPortfolioId }, portfolio);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Update a photo portfolio owned by the current user
        /// </summary>
        /// <param name="id">The photo portfolio ID</param>
        /// <param name="updateDto">The photo portfolio update data</param>
        /// <returns>The updated photo portfolio</returns>
        [HttpPut("{id:int}")]
        public async Task<ActionResult<PhotoPortfolioDto>> UpdatePhotoPortfolio(int id, [FromBody] UpdatePhotoPortfolioDto updateDto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized("Invalid user token");
            }

            var portfolio = await _photoPortfolioService.UpdatePhotoPortfolioByUserAsync(id, userId, updateDto);
            if (portfolio == null)
            {
                return NotFound($"Photo portfolio with ID {id} not found or you don't have permission to update it");
            }

            return Ok(portfolio);
        }

        /// <summary>
        /// Delete a photo portfolio owned by the current user
        /// </summary>
        /// <param name="id">The photo portfolio ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("{id:int}")]
        public async Task<ActionResult> DeletePhotoPortfolio(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized("Invalid user token");
            }

            var success = await _photoPortfolioService.DeletePhotoPortfolioByUserAsync(id, userId);
            if (!success)
            {
                return NotFound($"Photo portfolio with ID {id} not found or you don't have permission to delete it");
            }

            return NoContent();
        }

        /// <summary>
        /// Delete all photo portfolios for the current user
        /// </summary>
        /// <returns>Success status</returns>
        [HttpDelete("all")]
        public async Task<ActionResult> DeleteAllPhotoPortfolios()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized("Invalid user token");
            }

            var success = await _photoPortfolioService.DeleteAllPhotoPortfoliosByUserIdAsync(userId);
            return Ok(new { success, message = success ? "All portfolios deleted successfully" : "No portfolios found to delete" });
        }

        /// <summary>
        /// Check if the current user has any portfolio photos
        /// </summary>
        /// <returns>Boolean indicating if user has portfolio</returns>
        [HttpGet("has-portfolio")]
        public async Task<ActionResult<bool>> HasPortfolio()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized("Invalid user token");
            }

            var hasPortfolio = await _photoPortfolioService.UserHasPortfolioAsync(userId);
            return Ok(hasPortfolio);
        }

        /// <summary>
        /// Get the count of portfolio photos for the current user
        /// </summary>
        /// <returns>Count of portfolio photos</returns>
        [HttpGet("count")]
        public async Task<ActionResult<int>> GetPortfolioCount()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized("Invalid user token");
            }

            var count = await _photoPortfolioService.GetPortfolioCountByUserIdAsync(userId);
            return Ok(count);
        }
    }
}