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
    public class CloudinaryController : ControllerBase
    {
        private readonly ICloudinaryService _cloudinaryService;
        private readonly ILogger<CloudinaryController> _logger;

        public CloudinaryController(ICloudinaryService cloudinaryService, ILogger<CloudinaryController> logger)
        {
            _cloudinaryService = cloudinaryService;
            _logger = logger;
        }

        /// <summary>
        /// Upload a single image to Cloudinary
        /// </summary>
        /// <param name="file">The image file to upload</param>
        /// <param name="uploadType">Type of upload: avatar, portfolio, blog, general</param>
        /// <param name="publicId">Optional custom public ID</param>
        /// <param name="overwrite">Whether to overwrite existing file</param>
        /// <returns>Upload result with URL and metadata</returns>
        [HttpPost("upload")]
        public async Task<ActionResult<CloudinaryUploadResponseDto>> UploadImage(
            IFormFile file,
            [FromForm] string uploadType = "general",
            [FromForm] string? publicId = null,
            [FromForm] bool overwrite = true)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { error = "No file provided", message = "Please select a file to upload" });
                }

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(new { error = "Invalid token", message = "Could not determine user ID" });
                }

                if (!_cloudinaryService.IsValidImage(file))
                {
                    return BadRequest(new { error = "Invalid file", message = "Only JPG, JPEG, PNG, and WEBP images up to 10MB are allowed" });
                }

                var requestDto = new CloudinaryUploadRequestDto
                {
                    UserId = userId,
                    UploadType = uploadType,
                    PublicId = publicId,
                    Overwrite = overwrite
                };

                var result = await _cloudinaryService.UploadImageAsync(file, requestDto);
                
                _logger.LogInformation("Image uploaded successfully for user {UserId}: {PublicId}", userId, result.PublicId);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading image");
                return StatusCode(500, new { error = "Upload failed", message = ex.Message });
            }
        }

        /// <summary>
        /// Upload multiple images to Cloudinary
        /// </summary>
        /// <param name="files">List of image files to upload</param>
        /// <param name="uploadType">Type of upload: avatar, portfolio, blog, general</param>
        /// <returns>Bulk upload result</returns>
        [HttpPost("upload-multiple")]
        public async Task<ActionResult<CloudinaryBulkUploadResponseDto>> UploadMultipleImages(
            List<IFormFile> files,
            [FromForm] string uploadType = "portfolio")
        {
            try
            {
                if (files == null || !files.Any())
                {
                    return BadRequest(new { error = "No files provided", message = "Please select files to upload" });
                }

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(new { error = "Invalid token", message = "Could not determine user ID" });
                }

                var requestDto = new CloudinaryUploadRequestDto
                {
                    UserId = userId,
                    UploadType = uploadType,
                    Overwrite = true
                };

                var result = await _cloudinaryService.UploadImagesAsync(files, requestDto);
                
                _logger.LogInformation("Bulk upload completed for user {UserId}: {SuccessCount} succeeded, {FailureCount} failed",
                    userId, result.SuccessCount, result.FailureCount);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading multiple images");
                return StatusCode(500, new { error = "Upload failed", message = ex.Message });
            }
        }

        /// <summary>
        /// Upload an image from a base64 string
        /// </summary>
        /// <param name="base64Image">Base64 encoded image</param>
        /// <param name="uploadType">Type of upload: avatar, portfolio, blog, general</param>
        /// <param name="publicId">Optional custom public ID</param>
        /// <returns>Upload result with URL and metadata</returns>
        [HttpPost("upload-base64")]
        public async Task<ActionResult<CloudinaryUploadResponseDto>> UploadBase64Image(
            [FromBody] string base64Image,
            [FromQuery] string uploadType = "general",
            [FromQuery] string? publicId = null)
        {
            try
            {
                if (string.IsNullOrEmpty(base64Image))
                {
                    return BadRequest(new { error = "No image data provided", message = "Please provide a base64 encoded image" });
                }

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(new { error = "Invalid token", message = "Could not determine user ID" });
                }

                var requestDto = new CloudinaryUploadRequestDto
                {
                    UserId = userId,
                    UploadType = uploadType,
                    PublicId = publicId,
                    Overwrite = true
                };

                var result = await _cloudinaryService.UploadBase64ImageAsync(base64Image, requestDto);
                
                _logger.LogInformation("Base64 image uploaded successfully for user {UserId}: {PublicId}", userId, result.PublicId);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading base64 image");
                return StatusCode(500, new { error = "Upload failed", message = ex.Message });
            }
        }

        /// <summary>
        /// Delete an image from Cloudinary
        /// </summary>
        /// <param name="publicId">The public ID of the image to delete</param>
        /// <returns>Delete result</returns>
        [HttpDelete("delete")]
        public async Task<ActionResult<CloudinaryDeleteResponseDto>> DeleteImage([FromQuery] string publicId)
        {
            try
            {
                if (string.IsNullOrEmpty(publicId))
                {
                    return BadRequest(new { error = "Public ID required", message = "Please provide the public ID of the image to delete" });
                }

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(new { error = "Invalid token", message = "Could not determine user ID" });
                }

                // Verify the image belongs to the user (publicId should start with snapdi/{userId}/)
                if (!publicId.StartsWith($"snapdi/{userId}/"))
                {
                    return Forbid("You can only delete your own images");
                }

                var result = await _cloudinaryService.DeleteImageAsync(publicId);
                
                _logger.LogInformation("Image deleted for user {UserId}: {PublicId}", userId, publicId);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting image");
                return StatusCode(500, new { error = "Delete failed", message = ex.Message });
            }
        }

        /// <summary>
        /// Delete multiple images from Cloudinary
        /// </summary>
        /// <param name="publicIds">List of public IDs to delete</param>
        /// <returns>List of delete results</returns>
        [HttpDelete("delete-multiple")]
        public async Task<ActionResult<List<CloudinaryDeleteResponseDto>>> DeleteMultipleImages([FromBody] List<string> publicIds)
        {
            try
            {
                if (publicIds == null || !publicIds.Any())
                {
                    return BadRequest(new { error = "No public IDs provided", message = "Please provide public IDs to delete" });
                }

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(new { error = "Invalid token", message = "Could not determine user ID" });
                }

                // Verify all images belong to the user
                var unauthorizedIds = publicIds.Where(id => !id.StartsWith($"snapdi/{userId}/")).ToList();
                if (unauthorizedIds.Any())
                {
                    return Forbid($"You can only delete your own images. Unauthorized IDs: {string.Join(", ", unauthorizedIds)}");
                }

                var results = await _cloudinaryService.DeleteImagesAsync(publicIds);
                
                _logger.LogInformation("Multiple images deleted for user {UserId}: {Count} images", userId, publicIds.Count);
                
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting multiple images");
                return StatusCode(500, new { error = "Delete failed", message = ex.Message });
            }
        }

        /// <summary>
        /// Delete all images for a specific upload type
        /// </summary>
        /// <param name="uploadType">The upload type (avatar, portfolio, blog, general)</param>
        /// <returns>Delete result</returns>
        [HttpDelete("delete-all/{uploadType}")]
        public async Task<ActionResult<CloudinaryDeleteResponseDto>> DeleteAllImagesOfType(string uploadType)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(new { error = "Invalid token", message = "Could not determine user ID" });
                }

                var result = await _cloudinaryService.DeleteUserImagesAsync(userId, uploadType);
                
                _logger.LogInformation("All {UploadType} images deleted for user {UserId}", uploadType, userId);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting all images of type");
                return StatusCode(500, new { error = "Delete failed", message = ex.Message });
            }
        }

        /// <summary>
        /// Generate signed upload parameters for client-side upload
        /// </summary>
        /// <param name="requestDto">Signed upload configuration</param>
        /// <returns>Signed upload parameters</returns>
        [HttpPost("generate-signature")]
        public async Task<ActionResult<CloudinarySignedUploadResponseDto>> GenerateSignature([FromBody] CloudinarySignedUploadRequestDto requestDto)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(new { error = "Invalid token", message = "Could not determine user ID" });
                }

                // Ensure the request is for the current user
                if (requestDto.UserId != userId)
                {
                    return Forbid("You can only generate signatures for your own uploads");
                }

                var result = await _cloudinaryService.GenerateSignedUploadParamsAsync(requestDto);
                
                _logger.LogInformation("Upload signature generated for user {UserId}", userId);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating signature");
                return StatusCode(500, new { error = "Signature generation failed", message = ex.Message });
            }
        }

        /// <summary>
        /// Get a transformed image URL
        /// </summary>
        /// <param name="publicId">The public ID of the image</param>
        /// <param name="transformation">Transformation options</param>
        /// <returns>Transformed image URL</returns>
        [HttpPost("transform-url")]
        [AllowAnonymous] // Public endpoint for getting transformed URLs
        public ActionResult<string> GetTransformedUrl([FromQuery] string publicId, [FromBody] ImageTransformationDto? transformation = null)
        {
            try
            {
                if (string.IsNullOrEmpty(publicId))
                {
                    return BadRequest(new { error = "Public ID required", message = "Please provide the public ID of the image" });
                }

                var url = _cloudinaryService.GetTransformedImageUrl(publicId, transformation);
                return Ok(new { url });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating transformed URL");
                return StatusCode(500, new { error = "URL generation failed", message = ex.Message });
            }
        }

        /// <summary>
        /// Validate if a file is a valid image
        /// </summary>
        /// <param name="file">The file to validate</param>
        /// <returns>Validation result</returns>
        [HttpPost("validate-image")]
        public ActionResult<bool> ValidateImage(IFormFile file)
        {
            try
            {
                if (file == null)
                {
                    return BadRequest(new { error = "No file provided", message = "Please select a file to validate" });
                }

                var isValid = _cloudinaryService.IsValidImage(file);
                return Ok(new 
                { 
                    isValid, 
                    message = isValid 
                        ? "Image is valid" 
                        : "Invalid image. Only JPG, JPEG, PNG, and WEBP images up to 10MB are allowed"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating image");
                return StatusCode(500, new { error = "Validation failed", message = ex.Message });
            }
        }
    }
}
