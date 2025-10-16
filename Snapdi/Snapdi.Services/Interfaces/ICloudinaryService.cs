using Microsoft.AspNetCore.Http;
using Snapdi.Services.DTOs;

namespace Snapdi.Services.Interfaces
{
    /// <summary>
    /// Service for handling Cloudinary image uploads and management
    /// </summary>
    public interface ICloudinaryService
    {
        /// <summary>
        /// Upload a single image to Cloudinary
        /// </summary>
        /// <param name="file">The image file to upload</param>
        /// <param name="requestDto">Upload configuration</param>
        /// <returns>Upload result with URL and metadata</returns>
        Task<CloudinaryUploadResponseDto> UploadImageAsync(IFormFile file, CloudinaryUploadRequestDto requestDto);

        /// <summary>
        /// Upload multiple images to Cloudinary
        /// </summary>
        /// <param name="files">List of image files to upload</param>
        /// <param name="requestDto">Upload configuration</param>
        /// <returns>Bulk upload result</returns>
        Task<CloudinaryBulkUploadResponseDto> UploadImagesAsync(IEnumerable<IFormFile> files, CloudinaryUploadRequestDto requestDto);

        /// <summary>
        /// Upload an image from a base64 string
        /// </summary>
        /// <param name="base64String">Base64 encoded image</param>
        /// <param name="requestDto">Upload configuration</param>
        /// <returns>Upload result with URL and metadata</returns>
        Task<CloudinaryUploadResponseDto> UploadBase64ImageAsync(string base64String, CloudinaryUploadRequestDto requestDto);

        /// <summary>
        /// Delete an image from Cloudinary
        /// </summary>
        /// <param name="publicId">The public ID of the image to delete</param>
        /// <returns>Delete result</returns>
        Task<CloudinaryDeleteResponseDto> DeleteImageAsync(string publicId);

        /// <summary>
        /// Delete multiple images from Cloudinary
        /// </summary>
        /// <param name="publicIds">List of public IDs to delete</param>
        /// <returns>List of delete results</returns>
        Task<List<CloudinaryDeleteResponseDto>> DeleteImagesAsync(IEnumerable<string> publicIds);

        /// <summary>
        /// Delete all images in a user's folder
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="uploadType">Optional upload type (avatar, portfolio, etc.)</param>
        /// <returns>Delete result</returns>
        Task<CloudinaryDeleteResponseDto> DeleteUserImagesAsync(int userId, string? uploadType = null);

        /// <summary>
        /// Generate signed upload parameters for client-side upload
        /// </summary>
        /// <param name="requestDto">Signed upload configuration</param>
        /// <returns>Signed upload parameters</returns>
        Task<CloudinarySignedUploadResponseDto> GenerateSignedUploadParamsAsync(CloudinarySignedUploadRequestDto requestDto);

        /// <summary>
        /// Get a transformed image URL
        /// </summary>
        /// <param name="publicId">The public ID of the image</param>
        /// <param name="transformation">Transformation options</param>
        /// <returns>Transformed image URL</returns>
        string GetTransformedImageUrl(string publicId, ImageTransformationDto? transformation = null);

        /// <summary>
        /// Validate if a file is a valid image
        /// </summary>
        /// <param name="file">The file to validate</param>
        /// <returns>True if valid, false otherwise</returns>
        bool IsValidImage(IFormFile file);

        /// <summary>
        /// Get the folder path for a specific upload type and user
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="uploadType">The upload type</param>
        /// <returns>Folder path</returns>
        string GetFolderPath(int userId, string uploadType);
    }
}
