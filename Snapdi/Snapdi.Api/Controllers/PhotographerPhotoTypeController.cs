using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Snapdi.Services.DTOs;
using Snapdi.Services.Interfaces;
using System.Security.Claims;

namespace Snapdi.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PhotographerPhotoTypeController : ControllerBase
    {
        private readonly IPhotographerPhotoTypeService _photographerPhotoTypeService;
        private readonly IUserService _userService;

        public PhotographerPhotoTypeController(
            IPhotographerPhotoTypeService photographerPhotoTypeService,
            IUserService userService)
        {
            _photographerPhotoTypeService = photographerPhotoTypeService;
            _userService = userService;
        }

        /// <summary>
        /// Get all photo types with current user's pricing (for photographers)
        /// Returns all available photo types, with photographer's current prices if they have set them
        /// </summary>
        [HttpGet("my-photo-types")]
        [Authorize(Roles = "PHOTOGRAPHER")]
        public async Task<ActionResult<IEnumerable<PhotoTypeWithPricingResponseDto>>> GetMyPhotoTypes()
        {
            try
            {
                var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (currentUserIdClaim == null || !int.TryParse(currentUserIdClaim.Value, out int currentUserId))
                {
                    return BadRequest(new { error = "Invalid token", message = "Could not determine current user" });
                }

                // Verify user is a photographer
                var user = await _userService.GetUserWithPhotographerProfileAsync(currentUserId);
                if (user == null || user.PhotographerProfile == null)
                {
                    return StatusCode(403, new { error = "Access denied", message = "Only photographers can access this endpoint" });
                }

                var photoTypes = await _photographerPhotoTypeService.GetPhotoTypesWithPricingForUserAsync(currentUserId);
                return Ok(photoTypes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", message = "An error occurred while retrieving photo types", details = ex.Message });
            }
        }

        /// <summary>
        /// Update photo type prices for current photographer
        /// After successful update, automatically sets levelPhotographer to null for admin review
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// [
        ///   {
        ///     "photoTypeId": 1,
        ///     "photoPrice": 500000,
        ///     "time": 2
        ///   },
        ///   {
        ///     "photoTypeId": 2,
        ///     "photoPrice": 800000,
        ///     "time": 3
        ///   }
        /// ]
        /// 
        /// All prices and times must be greater than 0.
        /// </remarks>
        [HttpPut("update-prices")]
        public async Task<ActionResult> UpdatePrices([FromBody] List<PhotoTypeWithPricingDto> photoTypeDtos)
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

                var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (currentUserIdClaim == null || !int.TryParse(currentUserIdClaim.Value, out int currentUserId))
                {
                    return BadRequest(new { error = "Invalid token", message = "Could not determine current user" });
                }

                // Verify user is a photographer
                var user = await _userService.GetUserWithPhotographerProfileAsync(currentUserId);
                if (user == null || user.PhotographerProfile == null)
                {
                    return StatusCode(403, new { error = "Access denied", message = "Only photographers can update prices" });
                }

                if (photoTypeDtos == null || !photoTypeDtos.Any())
                {
                    return BadRequest(new { error = "Invalid request", message = "At least one photo type with pricing must be provided" });
                }

                var result = await _photographerPhotoTypeService.UpdatePricesAndResetLevelAsync(currentUserId, photoTypeDtos);
                if (!result)
                {
                    return BadRequest(new { error = "Update failed", message = "Failed to update photo type prices" });
                }

                return Ok(new
                {
                    userId = currentUserId,
                    message = "Photo type prices updated successfully. Level has been reset for admin review.",
                    updatedPhotoTypes = photoTypeDtos.Count
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = "Validation error", message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", message = "An error occurred while updating photo type prices", details = ex.Message });
            }
        }
    }
}

