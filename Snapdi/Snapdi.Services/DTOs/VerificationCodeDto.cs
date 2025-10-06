using System.ComponentModel.DataAnnotations;

namespace Snapdi.Services.DTOs
{
    /// <summary>
    /// DTO for email verification using verification code
    /// </summary>
    public class VerifyEmailWithCodeDto
    {
        /// <summary>
        /// User's email address
        /// </summary>
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Please provide a valid email address")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// 6-digit verification code
        /// </summary>
        [Required(ErrorMessage = "Verification code is required")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Verification code must be exactly 6 digits")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "Verification code must be 6 digits")]
        public string Code { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for resending verification code
    /// </summary>
    public class ResendVerificationCodeDto
    {
        /// <summary>
        /// User's email address to resend verification code to
        /// </summary>
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Please provide a valid email address")]
        public string Email { get; set; } = string.Empty;
    }
}