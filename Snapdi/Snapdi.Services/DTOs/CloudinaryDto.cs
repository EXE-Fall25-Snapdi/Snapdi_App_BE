using System.ComponentModel.DataAnnotations;

namespace Snapdi.Services.DTOs
{
    /// <summary>
    /// Upload request DTO for Cloudinary image upload
    /// </summary>
    public class CloudinaryUploadRequestDto
    {
        /// <summary>
        /// User ID who owns this file
        /// </summary>
        [Required]
        public int UserId { get; set; }

        /// <summary>
        /// Type of upload: 'avatar', 'portfolio', 'blog', 'general'
        /// </summary>
        [Required]
        public string UploadType { get; set; } = string.Empty;

        /// <summary>
        /// Optional custom public ID (filename without extension)
        /// If not provided, a unique ID will be generated
        /// </summary>
        public string? PublicId { get; set; }

        /// <summary>
        /// Whether to overwrite existing file with same public ID
        /// </summary>
        public bool Overwrite { get; set; } = true;

        /// <summary>
        /// Optional transformation to apply (e.g., "w_400,h_400,c_fill")
        /// </summary>
        public string? Transformation { get; set; }

        /// <summary>
        /// Optional tags for the upload
        /// </summary>
        public List<string>? Tags { get; set; }
    }

    /// <summary>
    /// Response DTO for successful Cloudinary upload
    /// </summary>
    public class CloudinaryUploadResponseDto
    {
        public string PublicId { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string SecureUrl { get; set; } = string.Empty;
        public string Format { get; set; } = string.Empty;
        public int Width { get; set; }
        public int Height { get; set; }
        public long Bytes { get; set; }
        public string ResourceType { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string? Signature { get; set; }
        public string? Etag { get; set; }
    }

    /// <summary>
    /// Response DTO for Cloudinary delete operation
    /// </summary>
    public class CloudinaryDeleteResponseDto
    {
        public bool Success { get; set; }
        public string Result { get; set; } = string.Empty;
        public string PublicId { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request DTO for generating signed upload parameters
    /// </summary>
    public class CloudinarySignedUploadRequestDto
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public string UploadType { get; set; } = string.Empty;

        public string? PublicId { get; set; }
        public bool Overwrite { get; set; } = true;
        public List<string>? Tags { get; set; }
    }

    /// <summary>
    /// Response DTO for signed upload parameters (for client-side upload)
    /// </summary>
    public class CloudinarySignedUploadResponseDto
    {
        public string ApiKey { get; set; } = string.Empty;
        public string CloudName { get; set; } = string.Empty;
        public string Signature { get; set; } = string.Empty;
        public long Timestamp { get; set; }
        public string Folder { get; set; } = string.Empty;
        public string? PublicId { get; set; }
        public string? Tags { get; set; }
        public bool Overwrite { get; set; }
        public string UploadPreset { get; set; } = string.Empty;
        public Dictionary<string, string> UploadParams { get; set; } = new();
    }

    /// <summary>
    /// Image transformation options
    /// </summary>
    public class ImageTransformationDto
    {
        public int? Width { get; set; }
        public int? Height { get; set; }
        public string? Crop { get; set; } // fill, scale, fit, etc.
        public string? Gravity { get; set; } // face, center, auto, etc.
        public int? Quality { get; set; }
        public string? Format { get; set; } // jpg, png, webp, auto
        public bool AutoOptimize { get; set; } = true;
    }

    /// <summary>
    /// Bulk upload response
    /// </summary>
    public class CloudinaryBulkUploadResponseDto
    {
        public List<CloudinaryUploadResponseDto> SuccessfulUploads { get; set; } = new();
        public List<CloudinaryUploadErrorDto> FailedUploads { get; set; } = new();
        public int TotalProcessed { get; set; }
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
    }

    /// <summary>
    /// Upload error details
    /// </summary>
    public class CloudinaryUploadErrorDto
    {
        public string FileName { get; set; } = string.Empty;
        public string Error { get; set; } = string.Empty;
        public string? Details { get; set; }
    }
}
