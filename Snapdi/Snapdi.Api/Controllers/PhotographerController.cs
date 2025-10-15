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
