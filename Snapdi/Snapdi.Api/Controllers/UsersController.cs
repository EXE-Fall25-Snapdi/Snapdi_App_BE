using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Snapdi.Services.DTOs;
using Snapdi.Services.Interfaces;
using Snapdi.Services.Constants;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;

namespace Snapdi.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Get photographers pending level assignment with paging and filtering (Admin only)
        /// </summary>
        /// <remarks>
        /// Returns photographers who need level assignment with advanced paging, filtering and sorting options.
        /// 
        /// Sample request:
        /// {
        ///   "page": 1,
        ///   "pageSize": 10,
        ///   "searchTerm": "john",
        ///   "hasPortfolio": true,
        ///   "locationCity": "Ho Chi Minh",
        ///   "sortBy": "createdAt",
        ///   "sortDirection": "desc",
        ///   "createdFrom": "2024-01-01T00:00:00Z",
        ///   "createdTo": "2024-12-31T23:59:59Z"
        /// }
        /// 
        /// Response includes paginated results for both with/without portfolio groups and summary statistics.
        /// </remarks>
        [HttpPost("photographers/pending-level/search")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<PhotograhpersPendingLevelPagedResponseDto>> GetPhotographersPendingLevelAssignmentPaged(
            [FromBody] GetPhotographersPendingLevelRequestDto request)
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

                var response = await _userService.GetPhotographersPendingLevelAssignmentPagedAsync(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    error = "Internal server error", 
                    message = "An error occurred while retrieving photographers pending level assignment", 
                    details = ex.Message 
                });
            }
        }

        /// <summary>
        /// Get photographers pending level assignment grouped by portfolio status (Admin only)
        /// </summary>
        [HttpGet("photographers/pending-level/grouped")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<PhotograhpersPendingLevelResponseDto>> GetPhotographersPendingLevelAssignmentGrouped()
        {
            try
            {
                var response = await _userService.GetPhotographersPendingLevelAssignmentGroupedAsync();
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", message = "An error occurred while retrieving photographers pending level assignment grouped by portfolio status", details = ex.Message });
            }
        }

        /// <summary>
        /// Get available photographer levels (Admin only)
        /// </summary>
        [HttpGet("photographers/levels")]
        [Authorize(Roles = "ADMIN")]
        public ActionResult<string[]> GetAvailablePhotographerLevels()
        {
            return Ok(new { 
                levels = PhotographerLevels.AllLevels,
                message = "Available photographer levels that can be assigned to photographers"
            });
        }

        /// <summary>
        /// Update photographer level (Admin only)
        /// </summary>
        [HttpPatch("{id}/photographer-level")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult> UpdatePhotographerLevel(int id, [FromBody] UpdatePhotographerLevelDto updateLevelDto)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { error = "Invalid user ID", message = "User ID must be a positive number" });
                }

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

                // Validate photographer level
                if (!PhotographerLevels.IsValidLevel(updateLevelDto.LevelPhotographer))
                {
                    return BadRequest(new { 
                        error = "Invalid photographer level", 
                        message = $"Level '{updateLevelDto.LevelPhotographer}' is not valid. Available levels: {string.Join(", ", PhotographerLevels.AllLevels)}"
                    });
                }

                var photographer = await _userService.GetUserWithPhotographerProfileAsync(id);
                if (photographer == null || photographer.PhotographerProfile == null)
                {
                    return NotFound(new { error = "Photographer not found", message = $"User with ID {id} does not exist or has no photographer profile" });
                }

                var result = await _userService.UpdatePhotographerLevelAsync(id, updateLevelDto.LevelPhotographer);
                if (!result)
                {
                    return BadRequest(new { error = "Update failed", message = "Failed to update photographer level" });
                }

                return Ok(new { 
                    message = "Photographer level updated successfully",
                    userId = id,
                    newLevel = updateLevelDto.LevelPhotographer
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", message = "An error occurred while updating photographer level", details = ex.Message });
            }
        }
    }

    /// <summary>
    /// DTO for updating photographer level (Admin only)
    /// </summary>
    public class UpdatePhotographerLevelDto
    {
        /// <summary>
        /// Photographer level - must be one of the predefined levels
        /// Available levels: "Beginner", "Intermediate", "Advanced", "Professional", "Expert"
        /// </summary>
        /// <example>Professional</example>
        [Required(ErrorMessage = "Level photographer is required")]
        [MaxLength(50, ErrorMessage = "Level photographer cannot exceed 50 characters")]
        public string LevelPhotographer { get; set; } = string.Empty;
    }
}