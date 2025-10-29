using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Snapdi.Services.DTOs;
using Snapdi.Services.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace Snapdi.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PhotographerController : ControllerBase
    {
        private readonly IUserService _userService;

        public PhotographerController(IUserService userService)
        {
            _userService = userService;
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
                    return BadRequest(new
                    {
                        error = "Validation failed",
                        message = "Please check your input data",
                        details = ModelState.Where(x => x.Value.Errors.Count > 0)
                            .ToDictionary(
                                kvp => kvp.Key,
                                kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                            )
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

                return Ok(new
                {
                    userId = id,
                    newLevel = updateLevelDto.LevelPhotographer
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", message = "An error occurred while updating photographer level", details = ex.Message });
            }
        }

        /// <summary>
        /// Update photographer availability status and location
        /// Photographers can update their own status, Admin can update any photographer's status
        /// </summary>
        /// <remarks>
        /// Updates photographer availability and optionally their current GPS location.
        /// 
        /// Sample request:
        /// {
        ///   "isAvailable": true,
        ///   "currentLocation": {
        ///     "latitude": 10.762622,
        ///     "longitude": 106.660172
        ///   }
        /// }
        /// 
        /// The currentLocation is optional. If not provided, only the availability status is updated.
        /// </remarks>
        [HttpPatch("{id}/status")]
        [Authorize]
        public async Task<ActionResult> UpdatePhotographerStatus(int id, [FromBody] UpdatePhotographerStatusDto statusDto)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { error = "Invalid user ID", message = "User ID must be a positive number" });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        error = "Validation failed",
                        message = "Please check your input data",
                        details = ModelState.Where(x => x.Value.Errors.Count > 0)
                            .ToDictionary(
                                kvp => kvp.Key,
                                kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                            )
                    });
                }

                // Check if photographer exists
                var photographer = await _userService.GetUserWithPhotographerProfileAsync(id);
                if (photographer == null || photographer.PhotographerProfile == null)
                {
                    return NotFound(new { error = "Photographer not found", message = $"User with ID {id} does not exist or has no photographer profile" });
                }

                // Check authorization: user can update their own status OR admin can update any
                var currentUserIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                var currentUserRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

                if (currentUserIdClaim != null && int.TryParse(currentUserIdClaim.Value, out int currentUserId))
                {
                    // User can update their own status OR admin can update any photographer's status
                    if (currentUserId != id && currentUserRole != "ADMIN")
                    {
                        return Forbid();
                    }
                }
                else
                {
                    return BadRequest(new { error = "Invalid token", message = "Could not determine current user" });
                }

                var result = await _userService.UpdatePhotographerStatusAsync(id, statusDto.IsAvailable, statusDto.CurrentLocation);
                if (!result)
                {
                    return BadRequest(new { error = "Update failed", message = "Failed to update photographer status" });
                }

                return Ok(new
                {
                    userId = id,
                    isAvailable = statusDto.IsAvailable,
                    locationUpdated = statusDto.CurrentLocation != null,
                    message = $"Photographer status updated successfully to {(statusDto.IsAvailable ? "available" : "unavailable")}" +
                              (statusDto.CurrentLocation != null ? " with location updated" : "")
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", message = "An error occurred while updating photographer status", details = ex.Message });
            }
        }

        /// <summary>
        /// Update photographer profile information (Description and WorkLocation)
        /// Photographers can update their own profile, Admin can update any photographer's profile
        /// </summary>
        /// <remarks>
        /// Updates photographer description and work location.
        /// 
        /// Sample request:
        /// {
        ///   "description": "Professional wedding and portrait photographer with 5+ years experience",
        ///   "workLocation": "Ho Chi Minh City, Vietnam"
        /// }
        /// 
        /// Both fields are optional. Only provided fields will be updated.
        /// </remarks>
        [HttpPatch("{id}/profile")]
        [Authorize]
        public async Task<ActionResult> UpdatePhotographerProfile(int id, [FromBody] UpdatePhotographerInfoDto updateDto)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { error = "Invalid user ID", message = "User ID must be a positive number" });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        error = "Validation failed",
                        message = "Please check your input data",
                        details = ModelState.Where(x => x.Value.Errors.Count > 0)
                            .ToDictionary(
                                kvp => kvp.Key,
                                kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                            )
                    });
                }

                // Check if photographer exists
                var photographer = await _userService.GetUserWithPhotographerProfileAsync(id);
                if (photographer == null || photographer.PhotographerProfile == null)
                {
                    return NotFound(new { error = "Photographer not found", message = $"User with ID {id} does not exist or has no photographer profile" });
                }

                // Check authorization: user can update their own profile OR admin can update any
                var currentUserIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                var currentUserRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

                if (currentUserIdClaim != null && int.TryParse(currentUserIdClaim.Value, out int currentUserId))
                {
                    // User can update their own profile OR admin can update any photographer's profile
                    if (currentUserId != id && currentUserRole != "ADMIN")
                    {
                        return Forbid();
                    }
                }
                else
                {
                    return BadRequest(new { error = "Invalid token", message = "Could not determine current user" });
                }

                var result = await _userService.UpdatePhotographerProfileAsync(id, updateDto);
                if (!result)
                {
                    return BadRequest(new { error = "Update failed", message = "Failed to update photographer profile" });
                }

                return Ok(new
                {
                    userId = id,
                    description = updateDto.Description,
                    workLocation = updateDto.WorkLocation,
                    message = "Photographer profile updated successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", message = "An error occurred while updating photographer profile", details = ex.Message });
            }
        }

        /// <summary>
        /// Get photographer availability status
        /// </summary>
        [HttpGet("{id}/availability")]
        [AllowAnonymous]
        public async Task<ActionResult> GetPhotographerAvailability(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { error = "Invalid user ID", message = "User ID must be a positive number" });
                }

                var photographer = await _userService.GetUserWithPhotographerProfileAsync(id);
                if (photographer == null || photographer.PhotographerProfile == null)
                {
                    return NotFound(new { error = "Photographer not found", message = $"User with ID {id} does not exist or has no photographer profile" });
                }

                return Ok(new
                {
                    userId = id,
                    name = photographer.Name,
                    isAvailable = photographer.PhotographerProfile.IsAvailable,
                    avgRating = photographer.PhotographerProfile.AvgRating,
                    levelPhotographer = photographer.PhotographerProfile.LevelPhotographer
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", message = "An error occurred while retrieving photographer availability", details = ex.Message });
            }
        }

        /// <summary>
        /// Get current authenticated user's availability status (for photographers)
        /// </summary>
        [HttpGet("me/availability")]
        [Authorize]
        public async Task<ActionResult> GetMyAvailability()
        {
            try
            {
                var currentUserIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (currentUserIdClaim == null || !int.TryParse(currentUserIdClaim.Value, out int currentUserId))
                {
                    return BadRequest(new { error = "Invalid token", message = "Could not determine current user" });
                }

                var photographer = await _userService.GetUserWithPhotographerProfileAsync(currentUserId);
                if (photographer == null || photographer.PhotographerProfile == null)
                {
                    return NotFound(new { error = "Photographer profile not found", message = "Current user does not have a photographer profile" });
                }

                return Ok(new
                {
                    userId = currentUserId,
                    name = photographer.Name,
                    email = photographer.Email,
                    isAvailable = photographer.PhotographerProfile.IsAvailable,
                    avgRating = photographer.PhotographerProfile.AvgRating,
                    levelPhotographer = photographer.PhotographerProfile.LevelPhotographer,
                    currentLocation = photographer.CurrentLocation
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", message = "An error occurred while retrieving availability status", details = ex.Message });
            }
        }

        /// <summary>
        /// Search photographers with advanced filtering (POST method)
        /// </summary>
        /// <remarks>
        /// Advanced search for photographers with comprehensive filtering options.
        /// 
        /// Sample request:
        /// {
        ///   "searchTerm": "john",
        ///   "locationCity": "Ho Chi Minh",
        ///   "levelPhotographer": "Professional",
        ///   "isAvailable": true,
        ///   "isVerify": true,
        ///   "isActive": true,
        ///   "minRating": 4.0,
        ///   "maxRating": 5.0,
        ///   "yearsOfExperience": "5+",
        ///   "hasPortfolio": true,
        ///   "createdFrom": "2024-01-01T00:00:00Z",
        ///   "createdTo": "2024-12-31T23:59:59Z",
        ///   "sortBy": "rating",
        ///   "sortDirection": "desc",
        ///   "pageNumber": 1,
        ///   "pageSize": 10
        /// }
        /// 
        /// All filters are optional. The endpoint returns photographers with their profiles and portfolio information.
        /// </remarks>
        [HttpPost("photographers/search")]
        public async Task<ActionResult<PhotographerSearchResultDto>> SearchPhotographers([FromBody] PhotographerSearchDto searchDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        error = "Validation failed",
                        message = "Please check your input data",
                        details = ModelState.Where(x => x.Value.Errors.Count > 0)
                            .ToDictionary(
                                kvp => kvp.Key,
                                kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                            )
                    });
                }

                // Validate and normalize pagination
                if (searchDto.PageNumber < 1) searchDto.PageNumber = 1;
                if (searchDto.PageSize < 1 || searchDto.PageSize > 100) searchDto.PageSize = 10;

                // Validate rating range
                if (searchDto.MinRating.HasValue && searchDto.MaxRating.HasValue &&
                    searchDto.MinRating.Value > searchDto.MaxRating.Value)
                {
                    return BadRequest(new
                    {
                        error = "Invalid rating range",
                        message = "Minimum rating cannot be greater than maximum rating"
                    });
                }

                // Validate date range
                if (searchDto.CreatedFrom.HasValue && searchDto.CreatedTo.HasValue &&
                    searchDto.CreatedFrom.Value > searchDto.CreatedTo.Value)
                {
                    return BadRequest(new
                    {
                        error = "Invalid date range",
                        message = "Created from date cannot be after created to date"
                    });
                }

                var result = await _userService.SearchPhotographersAsync(searchDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "Internal server error",
                    message = "An error occurred while searching photographers",
                    details = ex.Message
                });
            }
        }
    }
}
