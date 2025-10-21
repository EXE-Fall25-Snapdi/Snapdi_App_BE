using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Snapdi.Services.DTOs;
using Snapdi.Services.Interfaces;
using System.Security.Claims;

namespace Snapdi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StylesController : ControllerBase
{
    private readonly IStyleService _styleService;

    public StylesController(IStyleService styleService)
    {
        _styleService = styleService;
    }

    /// <summary>
    /// Get all available styles (Public endpoint)
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<StyleDto>>> GetAllStyles()
    {
        try
        {
            var styles = await _styleService.GetAllStylesAsync();
            return Ok(styles);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", message = "An error occurred while retrieving styles", details = ex.Message });
        }
    }

    /// <summary>
    /// Get style by ID (Public endpoint)
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<StyleDto>> GetStyle(int id)
    {
        try
        {
            if (id <= 0)
            {
                return BadRequest(new { error = "Invalid style ID", message = "Style ID must be a positive number" });
            }

            var style = await _styleService.GetStyleByIdAsync(id);
            if (style == null)
            {
                return NotFound(new { error = "Style not found", message = $"Style with ID {id} does not exist" });
            }

            return Ok(style);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", message = "An error occurred while retrieving the style", details = ex.Message });
        }
    }

    /// <summary>
    /// Create a new style (Admin only)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "ADMIN")]
    public async Task<ActionResult<StyleDto>> CreateStyle(CreateStyleDto createStyleDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { 
                    error = "Validation failed", 
                    message = "Please check your input data",
                    details = ModelState.Where(x => x.Value.Errors.Count > 0)
                        .ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                        )
                });
            }

            var style = await _styleService.CreateStyleAsync(createStyleDto);
            return CreatedAtAction(nameof(GetStyle), new { id = style.StyleId }, style);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = "Style already exists", message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", message = "An error occurred while creating the style", details = ex.Message });
        }
    }

    /// <summary>
    /// Update style (Admin only)
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<ActionResult<StyleDto>> UpdateStyle(int id, UpdateStyleDto updateStyleDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { 
                    error = "Validation failed", 
                    message = "Please check your input data",
                    details = ModelState.Where(x => x.Value.Errors.Count > 0)
                        .ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                        )
                });
            }

            if (id <= 0)
            {
                return BadRequest(new { error = "Invalid style ID", message = "Style ID must be a positive number" });
            }

            var style = await _styleService.UpdateStyleAsync(id, updateStyleDto);
            if (style == null)
            {
                return NotFound(new { error = "Style not found", message = $"Style with ID {id} does not exist" });
            }

            return Ok(style);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = "Style name already exists", message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", message = "An error occurred while updating the style", details = ex.Message });
        }
    }

    /// <summary>
    /// Delete style (Admin only)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<ActionResult> DeleteStyle(int id)
    {
        try
        {
            if (id <= 0)
            {
                return BadRequest(new { error = "Invalid style ID", message = "Style ID must be a positive number" });
            }

            var result = await _styleService.DeleteStyleAsync(id);
            if (!result)
            {
                return NotFound(new { error = "Style not found", message = $"Style with ID {id} does not exist" });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", message = "An error occurred while deleting the style", details = ex.Message });
        }
    }

    /// <summary>
    /// Check if style name exists (Public endpoint)
    /// </summary>
    [HttpGet("check-name")]
    public async Task<ActionResult<bool>> CheckStyleNameExists([FromQuery] string styleName, [FromQuery] int? excludeId = null)
    {
        try
        {
            if (string.IsNullOrEmpty(styleName))
            {
                return BadRequest(new { error = "Style name required", message = "Style name parameter is required" });
            }

            var exists = await _styleService.IsStyleNameExistsAsync(styleName, excludeId);
            return Ok(new { exists });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", message = "An error occurred while checking style name existence", details = ex.Message });
        }
    }
}
