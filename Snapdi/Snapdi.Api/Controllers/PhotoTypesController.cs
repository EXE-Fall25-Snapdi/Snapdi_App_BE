using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Snapdi.Services.DTOs;
using Snapdi.Services.Interfaces;

namespace Snapdi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PhotoTypesController : ControllerBase
{
    private readonly IPhotoTypeService _photoTypeService;

    public PhotoTypesController(IPhotoTypeService photoTypeService)
    {
        _photoTypeService = photoTypeService;
    }

    /// <summary>
    /// Get all available photo types (Public endpoint)
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PhotoTypeDto>>> GetAllPhotoTypes()
    {
        try
        {
            var photoTypes = await _photoTypeService.GetAllPhotoTypesAsync();
            return Ok(photoTypes);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", message = "An error occurred while retrieving photo types", details = ex.Message });
        }
    }

    /// <summary>
    /// Get photo type by ID (Public endpoint)
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<PhotoTypeDto>> GetPhotoType(int id)
    {
        try
        {
            if (id <= 0)
            {
                return BadRequest(new { error = "Invalid photo type ID", message = "Photo type ID must be a positive number" });
            }

            var photoType = await _photoTypeService.GetPhotoTypeByIdAsync(id);
            if (photoType == null)
            {
                return NotFound(new { error = "Photo type not found", message = $"Photo type with ID {id} does not exist" });
            }

            return Ok(photoType);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", message = "An error occurred while retrieving the photo type", details = ex.Message });
        }
    }

    /// <summary>
    /// Create a new photo type (Admin only)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "ADMIN")]
    public async Task<ActionResult<PhotoTypeDto>> CreatePhotoType(CreatePhotoTypeDto createPhotoTypeDto)
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

            var photoType = await _photoTypeService.CreatePhotoTypeAsync(createPhotoTypeDto);
            return CreatedAtAction(nameof(GetPhotoType), new { id = photoType.PhotoTypeId }, photoType);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = "Photo type already exists", message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", message = "An error occurred while creating the photo type", details = ex.Message });
        }
    }

    /// <summary>
    /// Update photo type (Admin only)
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<ActionResult<PhotoTypeDto>> UpdatePhotoType(int id, UpdatePhotoTypeDto updatePhotoTypeDto)
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
                return BadRequest(new { error = "Invalid photo type ID", message = "Photo type ID must be a positive number" });
            }

            var photoType = await _photoTypeService.UpdatePhotoTypeAsync(id, updatePhotoTypeDto);
            if (photoType == null)
            {
                return NotFound(new { error = "Photo type not found", message = $"Photo type with ID {id} does not exist" });
            }

            return Ok(photoType);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = "Photo type name already exists", message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", message = "An error occurred while updating the photo type", details = ex.Message });
        }
    }

    /// <summary>
    /// Delete photo type (Admin only)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<ActionResult> DeletePhotoType(int id)
    {
        try
        {
            if (id <= 0)
            {
                return BadRequest(new { error = "Invalid photo type ID", message = "Photo type ID must be a positive number" });
            }

            var result = await _photoTypeService.DeletePhotoTypeAsync(id);
            if (!result)
            {
                return NotFound(new { error = "Photo type not found", message = $"Photo type with ID {id} does not exist" });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", message = "An error occurred while deleting the photo type", details = ex.Message });
        }
    }

    /// <summary>
    /// Check if photo type name exists (Public endpoint)
    /// </summary>
    [HttpGet("check-name")]
    public async Task<ActionResult<bool>> CheckPhotoTypeNameExists([FromQuery] string photoTypeName, [FromQuery] int? excludeId = null)
    {
        try
        {
            if (string.IsNullOrEmpty(photoTypeName))
            {
                return BadRequest(new { error = "Photo type name required", message = "Photo type name parameter is required" });
            }

            var exists = await _photoTypeService.IsPhotoTypeNameExistsAsync(photoTypeName, excludeId);
            return Ok(new { exists });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", message = "An error occurred while checking photo type name existence", details = ex.Message });
        }
    }
}
