namespace Snapdi.Services.Models
{
    public class AppSettings
    {
        public string BaseUrl { get; set; } = string.Empty;
    }

    public class EmailSettings
    {
        public string SmtpHost { get; set; } = string.Empty;
        public int SmtpPort { get; set; }
        public string SmtpUsername { get; set; } = string.Empty;
        public string SmtpPassword { get; set; } = string.Empty;
        public string FromEmail { get; set; } = string.Empty;
        public string FromName { get; set; } = string.Empty;
    }

    public class JwtSettings
    {
        public string Key { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public int ExpirationHours { get; set; } = 1;
    }

    public class CloudinarySettings
    {
        public string CloudName { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public string ApiSecret { get; set; } = string.Empty;
        public string UploadPreset { get; set; } = "snapdi_default";
        public string FolderPath { get; set; } = "snapdi";
        public bool UseSignedUpload { get; set; } = true;
    }

    public class PayOSSettings
    {
        public string payOSClientId { get; set; } = string.Empty;
        public string payOSApiKey { get; set; } = string.Empty;
        public string payOSChecksumKey { get; set; } = string.Empty;
        public string payOSReturnUrl { get; set; } = string.Empty;

        public string payOSCancelUrl { get; set; } = string.Empty;

    }
}