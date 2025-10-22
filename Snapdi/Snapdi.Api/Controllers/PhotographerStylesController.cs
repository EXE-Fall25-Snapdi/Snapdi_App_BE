using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Snapdi.Services.DTOs;
using Snapdi.Services.Interfaces;
using System.Security.Claims;

namespace Snapdi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PhotographerStylesController : ControllerBase
{
    private readonly IPhotographerStyleService _photographerStyleService;

    public PhotographerStylesController(IPhotographerStyleService photographerStyleService)
    {
        _photographerStyleService = photographerStyleService;
    }

    /// <summary>
    /// Get styles for a specific photographer (Public endpoint)
    /// </summary>
    [HttpGet("photographer/{userId}")]
    public async Task<ActionResult<IEnumerable<StyleDto>>> GetPhotographerStyles(int userId)
    {
        try
        {
            if (userId <= 0)
            {
                return BadRequest(new { error = "Invalid user ID", message = "User ID must be a positive number" });
            }

            var styles = await _photographerStyleService.GetStylesByPhotographerAsync(userId);
            return Ok(styles);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", message = "An error occurred while retrieving photographer styles", details = ex.Message });
        }
    }

    /// <summary>
    /// Get photographer with their selected styles (Public endpoint)
    /// </summary>
    [HttpGet("photographer/{userId}/with-styles")]
    public async Task<ActionResult<PhotographerStyleResponseDto>> GetPhotographerWithStyles(int userId)
    {
        try
        {
            if (userId <= 0)
            {
                return BadRequest(new { error = "Invalid user ID", message = "User ID must be a positive number" });
            }

            var photographer = await _photographerStyleService.GetPhotographerWithStylesAsync(userId);
            if (photographer == null)
            {
                return NotFound(new { error = "Photographer not found", message = $"Photographer with ID {userId} does not exist" });
            }

            return Ok(photographer);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", message = "An error occurred while retrieving photographer with styles", details = ex.Message });
        }
    }

    /// <summary>
    /// Get style selection for photographer (shows all styles with selection status) (Authenticated users)
    /// </summary>
    [HttpGet("photographer/{userId}/selection")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<StyleSelectionDto>>> GetStyleSelectionForPhotographer(int userId)
    {
        try
        {
            if (userId <= 0)
            {
                return BadRequest(new { error = "Invalid user ID", message = "User ID must be a positive number" });
            }

            // Check if user can access this photographer's data
            var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;
            
            if (currentUserIdClaim != null && int.TryParse(currentUserIdClaim.Value, out int currentUserId))
            {
                // User can access their own data OR admin can access any data
                if (currentUserId != userId && currentUserRole != "ADMIN")
                {
                    return Forbid("You can only access your own photographer styles unless you are an admin");
                }
            }
            else
            {
                return BadRequest(new { error = "Invalid token", message = "Could not determine current user" });
            }

            var styleSelection = await _photographerStyleService.GetStyleSelectionForPhotographerAsync(userId);
            return Ok(styleSelection);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", message = "An error occurred while retrieving style selection", details = ex.Message });
        }
    }

    /// <summary>
    /// Add style to photographer (Authenticated users)
    /// </summary>
    [HttpPost("photographer/{userId}/style/{styleId}")]
    [Authorize]
    public async Task<ActionResult> AddStyleToPhotographer(int userId, int styleId)
    {
        try
        {
            if (userId <= 0)
            {
                return BadRequest(new { error = "Invalid user ID", message = "User ID must be a positive number" });
            }

            if (styleId <= 0)
            {
                return BadRequest(new { error = "Invalid style ID", message = "Style ID must be a positive number" });
            }

            // Check if user can modify this photographer's data
            var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;
            
            if (currentUserIdClaim != null && int.TryParse(currentUserIdClaim.Value, out int currentUserId))
            {
                // User can modify their own data OR admin can modify any data
                if (currentUserId != userId && currentUserRole != "ADMIN")
                {
                    return Forbid("You can only modify your own photographer styles unless you are an admin");
                }
            }
            else
            {
                return BadRequest(new { error = "Invalid token", message = "Could not determine current user" });
            }

            var result = await _photographerStyleService.AddStyleToPhotographerAsync(userId, styleId);
            if (!result)
            {
                return BadRequest(new { error = "Failed to add style", message = "Style may already be assigned to this photographer" });
            }

            return Ok(new { message = "Style added to photographer successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", message = "An error occurred while adding style to photographer", details = ex.Message });
        }
    }

    /// <summary>
    /// Add multiple styles to photographer (Authenticated users)
    /// </summary>
    [HttpPost("photographer/{userId}/styles/multiple")]
    [Authorize]
    public async Task<ActionResult<MultipleStyleOperationResponseDto>> AddMultipleStylesToPhotographer(int userId, [FromBody] AddMultipleStylesDto addStylesDto)
    {
        try
        {
            if (userId <= 0)
            {
                return BadRequest(new { error = "Invalid user ID", message = "User ID must be a positive number" });
            }

            if (addStylesDto?.StyleIds == null || !addStylesDto.StyleIds.Any())
            {
                return BadRequest(new { error = "Style IDs required", message = "At least one style ID is required" });
            }

            // Check if user can modify this photographer's data
            var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;
            
            if (currentUserIdClaim != null && int.TryParse(currentUserIdClaim.Value, out int currentUserId))
            {
                // User can modify their own data OR admin can modify any data
                if (currentUserId != userId && currentUserRole != "ADMIN")
                {
                    return Forbid("You can only modify your own photographer styles unless you are an admin");
                }
            }
            else
            {
                return BadRequest(new { error = "Invalid token", message = "Could not determine current user" });
            }

            // Remove duplicates and invalid IDs
            var validStyleIds = addStylesDto.StyleIds
                .Where(id => id > 0)
                .Distinct()
                .ToList();

            if (!validStyleIds.Any())
            {
                return BadRequest(new { error = "No valid style IDs", message = "No valid style IDs provided" });
            }

            var response = new MultipleStyleOperationResponseDto
            {
                TotalAttempted = validStyleIds.Count
            };

            // Get existing style IDs to avoid duplicates
            var existingStyles = await _photographerStyleService.GetStylesByPhotographerAsync(userId);
            var existingStyleIds = existingStyles.Select(s => s.StyleId).ToHashSet();

            var stylesToAdd = validStyleIds.Where(id => !existingStyleIds.Contains(id)).ToList();
            var alreadyExistingIds = validStyleIds.Where(id => existingStyleIds.Contains(id)).ToList();

            if (!stylesToAdd.Any())
            {
                response.SuccessCount = 0;
                response.FailedCount = validStyleIds.Count;
                response.FailedStyleIds = validStyleIds;
                response.Message = "All styles are already assigned to this photographer";
                return Ok(response);
            }

            var result = await _photographerStyleService.AddMultipleStylesToPhotographerAsync(userId, stylesToAdd);

            if (result)
            {
                response.SuccessCount = stylesToAdd.Count;
                response.SuccessfulStyleIds = stylesToAdd;
                response.FailedCount = alreadyExistingIds.Count;
                response.FailedStyleIds = alreadyExistingIds;
                response.Message = response.FailedCount > 0
                    ? $"Successfully added {response.SuccessCount} styles. {response.FailedCount} styles were already assigned."
                    : $"Successfully added {response.SuccessCount} styles to photographer.";
            }
            else
            {
                response.SuccessCount = 0;
                response.FailedCount = validStyleIds.Count;
                response.FailedStyleIds = validStyleIds;
                response.Message = "Failed to add styles to photographer";
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", message = "An error occurred while adding multiple styles to photographer", details = ex.Message });
        }
    }

    /// <summary>
    /// Add multiple styles to current authenticated photographer
    /// </summary>
    [HttpPost("my-styles/multiple")]
    [Authorize]
    public async Task<ActionResult<MultipleStyleOperationResponseDto>> AddMultipleStylesToCurrentPhotographer([FromBody] AddMultipleStylesDto addStylesDto)
    {
        try
        {
            var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (currentUserIdClaim == null || !int.TryParse(currentUserIdClaim.Value, out int currentUserId))
            {
                return BadRequest(new { error = "Invalid token", message = "Could not determine current user" });
            }

            // Call the existing method with current user ID
            return await AddMultipleStylesToPhotographer(currentUserId, addStylesDto);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", message = "An error occurred while adding multiple styles", details = ex.Message });
        }
    }

    /// <summary>
    /// Remove style from photographer (Authenticated users)
    /// </summary>
    [HttpDelete("photographer/{userId}/style/{styleId}")]
    [Authorize]
    public async Task<ActionResult> RemoveStyleFromPhotographer(int userId, int styleId)
    {
        try
        {
            if (userId <= 0)
            {
                return BadRequest(new { error = "Invalid user ID", message = "User ID must be a positive number" });
            }

            if (styleId <= 0)
            {
                return BadRequest(new { error = "Invalid style ID", message = "Style ID must be a positive number" });
            }

            // Check if user can modify this photographer's data
            var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;
            
            if (currentUserIdClaim != null && int.TryParse(currentUserIdClaim.Value, out int currentUserId))
            {
                // User can modify their own data OR admin can modify any data
                if (currentUserId != userId && currentUserRole != "ADMIN")
                {
                    return Forbid("You can only modify your own photographer styles unless you are an admin");
                }
            }
            else
            {
                return BadRequest(new { error = "Invalid token", message = "Could not determine current user" });
            }

            var result = await _photographerStyleService.RemoveStyleFromPhotographerAsync(userId, styleId);
            if (!result)
            {
                return NotFound(new { error = "Style not found", message = "Style is not assigned to this photographer" });
            }

            return Ok(new { message = "Style removed from photographer successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", message = "An error occurred while removing style from photographer", details = ex.Message });
        }
    }

    /// <summary>
    /// Update photographer styles (differential update - only add new and remove deleted styles) (Authenticated users)
    /// </summary>
    [HttpPut("photographer/{userId}/styles")]
    [Authorize]
    public async Task<ActionResult> UpdatePhotographerStyles(int userId, [FromBody] IEnumerable<int> styleIds)
    {
        try
        {
            if (userId <= 0)
            {
                return BadRequest(new { error = "Invalid user ID", message = "User ID must be a positive number" });
            }

            if (styleIds == null)
            {
                return BadRequest(new { error = "Style IDs required", message = "Style IDs array is required" });
            }

            // Check if user can modify this photographer's data
            var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;
            
            if (currentUserIdClaim != null && int.TryParse(currentUserIdClaim.Value, out int currentUserId))
            {
                // User can modify their own data OR admin can modify any data
                if (currentUserId != userId && currentUserRole != "ADMIN")
                {
                    return Forbid("You can only modify your own photographer styles unless you are an admin");
                }
            }
            else
            {
                return BadRequest(new { error = "Invalid token", message = "Could not determine current user" });
            }

            // Use differential update instead of replace all
            var result = await _photographerStyleService.UpdatePhotographerStylesDifferentialAsync(userId, styleIds);
            if (!result)
            {
                return BadRequest(new { error = "Failed to update styles", message = "An error occurred while updating photographer styles" });
            }

            return Ok(new { message = "Photographer styles updated successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", message = "An error occurred while updating photographer styles", details = ex.Message });
        }
    }

    /// <summary>
    /// Update current authenticated photographer's styles (differential update)
    /// </summary>
    [HttpPut("my-styles")]
    [Authorize]
    public async Task<ActionResult> UpdateCurrentPhotographerStyles([FromBody] IEnumerable<int> styleIds)
    {
        try
        {
            var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (currentUserIdClaim == null || !int.TryParse(currentUserIdClaim.Value, out int currentUserId))
            {
                return BadRequest(new { error = "Invalid token", message = "Could not determine current user" });
            }

            // Call the existing method with current user ID
            return await UpdatePhotographerStyles(currentUserId, styleIds);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", message = "An error occurred while updating photographer styles", details = ex.Message });
        }
    }

    /// <summary>
    /// Get photographers by style (Public endpoint)
    /// </summary>
    [HttpGet("style/{styleId}/photographers")]
    public async Task<ActionResult<IEnumerable<PhotographerStyleDto>>> GetPhotographersByStyle(int styleId)
    {
        try
        {
            if (styleId <= 0)
            {
                return BadRequest(new { error = "Invalid style ID", message = "Style ID must be a positive number" });
            }

            var photographers = await _photographerStyleService.GetPhotographersByStyleAsync(styleId);
            return Ok(photographers);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", message = "An error occurred while retrieving photographers by style", details = ex.Message });
        }
    }

    /// <summary>
    /// Check if photographer has specific style (Public endpoint)
    /// </summary>
    [HttpGet("photographer/{userId}/style/{styleId}/exists")]
    public async Task<ActionResult<bool>> CheckPhotographerStyleExists(int userId, int styleId)
    {
        try
        {
            if (userId <= 0)
            {
                return BadRequest(new { error = "Invalid user ID", message = "User ID must be a positive number" });
            }

            if (styleId <= 0)
            {
                return BadRequest(new { error = "Invalid style ID", message = "Style ID must be a positive number" });
            }

            var exists = await _photographerStyleService.IsPhotographerStyleExistsAsync(userId, styleId);
            return Ok(new { exists });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", message = "An error occurred while checking photographer style existence", details = ex.Message });
        }
    }

    /// <summary>
    /// Debug endpoint - Get detailed photographer styles info (Public endpoint for testing)
    /// </summary>
    [HttpGet("debug/photographer/{userId}/styles")]
    public async Task<ActionResult> DebugPhotographerStyles(int userId)
    {
        try
        {
            if (userId <= 0)
            {
                return BadRequest(new { error = "Invalid user ID", message = "User ID must be a positive number" });
            }

            var styles = await _photographerStyleService.GetStylesByPhotographerAsync(userId);
            var photographerWithStyles = await _photographerStyleService.GetPhotographerWithStylesAsync(userId);
            
            return Ok(new { 
                userId = userId,
                stylesCount = styles.Count(),
                styles = styles,
                photographerWithStyles = photographerWithStyles,
                message = "Debug information for photographer styles"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", message = "An error occurred while debugging photographer styles", details = ex.Message });
        }
    }
}
