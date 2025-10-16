using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Snapdi.Services.DTOs;
using Snapdi.Services.Interfaces;
using Snapdi.Services.Models;
using System.Security.Cryptography;
using System.Text;

namespace Snapdi.Services.Services
{
    /// <summary>
    /// Service for handling Cloudinary image uploads and management
    /// </summary>
    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;
        private readonly CloudinarySettings _settings;
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
        private readonly long _maxFileSizeBytes = 10 * 1024 * 1024; // 10MB

        public CloudinaryService(IOptions<CloudinarySettings> settings)
        {
            _settings = settings.Value;

            var account = new Account(
                _settings.CloudName,
                _settings.ApiKey,
                _settings.ApiSecret
            );

            _cloudinary = new Cloudinary(account);
        }

        public async Task<CloudinaryUploadResponseDto> UploadImageAsync(IFormFile file, CloudinaryUploadRequestDto requestDto)
        {
            // Validate the image
            if (!IsValidImage(file))
            {
                throw new InvalidOperationException("Invalid image file. Only JPG, JPEG, PNG, and WEBP formats are allowed.");
            }

            // Generate folder path and public ID
            var folder = GetFolderPath(requestDto.UserId, requestDto.UploadType);
            var publicId = string.IsNullOrEmpty(requestDto.PublicId)
                ? GeneratePublicId(requestDto.UploadType)
                : requestDto.PublicId;

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, file.OpenReadStream()),
                PublicId = publicId,
                Folder = folder,
                Overwrite = requestDto.Overwrite,
                UniqueFilename = string.IsNullOrEmpty(requestDto.PublicId), // Only use unique filename if no public ID provided
                UseFilename = !string.IsNullOrEmpty(requestDto.PublicId),
                Transformation = new Transformation()
                    .FetchFormat("auto")
                    .Quality("auto")
            };

            // Add custom transformation if provided
            if (!string.IsNullOrEmpty(requestDto.Transformation))
            {
                uploadParams.Transformation = new Transformation().RawTransformation(requestDto.Transformation);
            }

            // Add tags if provided
            if (requestDto.Tags != null && requestDto.Tags.Any())
            {
                uploadParams.Tags = string.Join(",", requestDto.Tags);
            }

            // Perform upload
            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.Error != null)
            {
                throw new Exception($"Cloudinary upload failed: {uploadResult.Error.Message}");
            }

            return MapToUploadResponse(uploadResult);
        }

        public async Task<CloudinaryBulkUploadResponseDto> UploadImagesAsync(IEnumerable<IFormFile> files, CloudinaryUploadRequestDto requestDto)
        {
            var response = new CloudinaryBulkUploadResponseDto();
            var fileList = files.ToList();
            response.TotalProcessed = fileList.Count;

            foreach (var file in fileList)
            {
                try
                {
                    // Create individual request for each file
                    var individualRequest = new CloudinaryUploadRequestDto
                    {
                        UserId = requestDto.UserId,
                        UploadType = requestDto.UploadType,
                        Overwrite = requestDto.Overwrite,
                        Transformation = requestDto.Transformation,
                        Tags = requestDto.Tags,
                        PublicId = null // Generate unique ID for each file
                    };

                    var result = await UploadImageAsync(file, individualRequest);
                    response.SuccessfulUploads.Add(result);
                    response.SuccessCount++;
                }
                catch (Exception ex)
                {
                    response.FailedUploads.Add(new CloudinaryUploadErrorDto
                    {
                        FileName = file.FileName,
                        Error = ex.Message,
                        Details = ex.InnerException?.Message
                    });
                    response.FailureCount++;
                }
            }

            return response;
        }

        public async Task<CloudinaryUploadResponseDto> UploadBase64ImageAsync(string base64String, CloudinaryUploadRequestDto requestDto)
        {
            // Remove data URI prefix if present
            var base64Data = base64String.Contains(",")
                ? base64String.Split(',')[1]
                : base64String;

            // Generate folder path and public ID
            var folder = GetFolderPath(requestDto.UserId, requestDto.UploadType);
            var publicId = string.IsNullOrEmpty(requestDto.PublicId)
                ? GeneratePublicId(requestDto.UploadType)
                : requestDto.PublicId;

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription($"data:image/png;base64,{base64Data}"),
                PublicId = publicId,
                Folder = folder,
                Overwrite = requestDto.Overwrite,
                UniqueFilename = string.IsNullOrEmpty(requestDto.PublicId),
                UseFilename = !string.IsNullOrEmpty(requestDto.PublicId),
                Transformation = new Transformation()
                    .FetchFormat("auto")
                    .Quality("auto")
            };

            // Add custom transformation if provided
            if (!string.IsNullOrEmpty(requestDto.Transformation))
            {
                uploadParams.Transformation = new Transformation().RawTransformation(requestDto.Transformation);
            }

            // Add tags if provided
            if (requestDto.Tags != null && requestDto.Tags.Any())
            {
                uploadParams.Tags = string.Join(",", requestDto.Tags);
            }

            // Perform upload
            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.Error != null)
            {
                throw new Exception($"Cloudinary upload failed: {uploadResult.Error.Message}");
            }

            return MapToUploadResponse(uploadResult);
        }

        public async Task<CloudinaryDeleteResponseDto> DeleteImageAsync(string publicId)
        {
            var deleteParams = new DeletionParams(publicId)
            {
                ResourceType = ResourceType.Image
            };

            var result = await _cloudinary.DestroyAsync(deleteParams);

            return new CloudinaryDeleteResponseDto
            {
                Success = result.Result == "ok",
                Result = result.Result,
                PublicId = publicId,
                Message = result.Result == "ok" ? "Image deleted successfully" : $"Delete failed: {result.Result}"
            };
        }

        public async Task<List<CloudinaryDeleteResponseDto>> DeleteImagesAsync(IEnumerable<string> publicIds)
        {
            var results = new List<CloudinaryDeleteResponseDto>();

            foreach (var publicId in publicIds)
            {
                try
                {
                    var result = await DeleteImageAsync(publicId);
                    results.Add(result);
                }
                catch (Exception ex)
                {
                    results.Add(new CloudinaryDeleteResponseDto
                    {
                        Success = false,
                        PublicId = publicId,
                        Result = "error",
                        Message = ex.Message
                    });
                }
            }

            return results;
        }

        public async Task<CloudinaryDeleteResponseDto> DeleteUserImagesAsync(int userId, string? uploadType = null)
        {
            var folder = string.IsNullOrEmpty(uploadType)
                ? $"{_settings.FolderPath}/{userId}"
                : GetFolderPath(userId, uploadType);

            var deleteParams = new DelResParams
            {
                Prefix = folder,
                ResourceType = ResourceType.Image
            };

            var result = await _cloudinary.DeleteResourcesByPrefixAsync(folder);

            return new CloudinaryDeleteResponseDto
            {
                Success = true,
                Result = "ok",
                PublicId = folder,
                Message = $"Deleted all images in folder: {folder}"
            };
        }

        public Task<CloudinarySignedUploadResponseDto> GenerateSignedUploadParamsAsync(CloudinarySignedUploadRequestDto requestDto)
        {
            var folder = GetFolderPath(requestDto.UserId, requestDto.UploadType);
            var publicId = string.IsNullOrEmpty(requestDto.PublicId)
                ? null
                : $"{folder}/{requestDto.PublicId}";

            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            // Build parameters for signing
            var uploadParams = new Dictionary<string, string>
            {
                { "timestamp", timestamp.ToString() },
                { "folder", folder },
                { "upload_preset", _settings.UploadPreset }
            };

            if (!string.IsNullOrEmpty(publicId))
            {
                uploadParams["public_id"] = publicId;
            }

            if (requestDto.Overwrite)
            {
                uploadParams["overwrite"] = "true";
            }

            if (requestDto.Tags != null && requestDto.Tags.Any())
            {
                uploadParams["tags"] = string.Join(",", requestDto.Tags);
            }

            // Generate signature
            var signature = GenerateSignature(uploadParams, _settings.ApiSecret);

            var response = new CloudinarySignedUploadResponseDto
            {
                ApiKey = _settings.ApiKey,
                CloudName = _settings.CloudName,
                Signature = signature,
                Timestamp = timestamp,
                Folder = folder,
                PublicId = publicId,
                Tags = requestDto.Tags != null && requestDto.Tags.Any() ? string.Join(",", requestDto.Tags) : null,
                Overwrite = requestDto.Overwrite,
                UploadPreset = _settings.UploadPreset,
                UploadParams = uploadParams
            };

            return Task.FromResult(response);
        }

        public string GetTransformedImageUrl(string publicId, ImageTransformationDto? transformation = null)
        {
            if (transformation == null)
            {
                return _cloudinary.Api.UrlImgUp.BuildUrl(publicId);
            }

            var trans = new Transformation();

            if (transformation.Width.HasValue)
                trans = trans.Width(transformation.Width.Value);

            if (transformation.Height.HasValue)
                trans = trans.Height(transformation.Height.Value);

            if (!string.IsNullOrEmpty(transformation.Crop))
                trans = trans.Crop(transformation.Crop);

            if (!string.IsNullOrEmpty(transformation.Gravity))
                trans = trans.Gravity(transformation.Gravity);

            if (transformation.Quality.HasValue)
                trans = trans.Quality(transformation.Quality.Value);

            if (!string.IsNullOrEmpty(transformation.Format))
                trans = trans.FetchFormat(transformation.Format);

            if (transformation.AutoOptimize)
            {
                trans = trans.FetchFormat("auto").Quality("auto");
            }

            return _cloudinary.Api.UrlImgUp.Transform(trans).BuildUrl(publicId);
        }

        public bool IsValidImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return false;

            if (file.Length > _maxFileSizeBytes)
                return false;

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_allowedExtensions.Contains(extension))
                return false;

            // Optional: Check file signature (magic bytes)
            // This prevents uploading files with fake extensions
            try
            {
                using var stream = file.OpenReadStream();
                var header = new byte[8];
                stream.Read(header, 0, header.Length);

                // Check for common image formats
                // JPG: FF D8 FF
                if (header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF)
                    return true;

                // PNG: 89 50 4E 47 0D 0A 1A 0A
                if (header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47)
                    return true;

                // WEBP: RIFF ... WEBP
                if (header[0] == 0x52 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x46)
                    return true;
            }
            catch
            {
                return false;
            }

            return false;
        }

        public string GetFolderPath(int userId, string uploadType)
        {
            var folder = $"{_settings.FolderPath}/{userId}";

            return uploadType.ToLower() switch
            {
                "avatar" => folder,
                "portfolio" => $"{folder}/portfolio",
                "blog" => $"{folder}/blog",
                "general" => $"{folder}/general",
                _ => $"{folder}/{uploadType}"
            };
        }

        #region Private Helper Methods

        private static CloudinaryUploadResponseDto MapToUploadResponse(ImageUploadResult uploadResult)
        {
            return new CloudinaryUploadResponseDto
            {
                PublicId = uploadResult.PublicId,
                Url = uploadResult.Url?.ToString() ?? string.Empty,
                SecureUrl = uploadResult.SecureUrl?.ToString() ?? string.Empty,
                Format = uploadResult.Format,
                Width = uploadResult.Width,
                Height = uploadResult.Height,
                Bytes = uploadResult.Bytes,
                ResourceType = uploadResult.ResourceType,
                CreatedAt = uploadResult.CreatedAt,
                Signature = uploadResult.Signature,
                Etag = uploadResult.Etag
            };
        }

        private static string GeneratePublicId(string uploadType)
        {
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var random = Guid.NewGuid().ToString("N").Substring(0, 8);
            return $"{uploadType}_{timestamp}_{random}";
        }

        private static string GenerateSignature(Dictionary<string, string> parameters, string apiSecret)
        {
            // Sort parameters alphabetically
            var sortedParams = parameters
                .OrderBy(kvp => kvp.Key)
                .Where(kvp => kvp.Key != "api_key" && kvp.Key != "resource_type" && kvp.Key != "cloud_name")
                .Select(kvp => $"{kvp.Key}={kvp.Value}");

            var stringToSign = string.Join("&", sortedParams) + apiSecret;

            using var sha1 = SHA1.Create();
            var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(stringToSign));
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }

        #endregion
    }
}
