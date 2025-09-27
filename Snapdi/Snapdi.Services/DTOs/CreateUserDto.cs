using System.ComponentModel.DataAnnotations;

namespace Snapdi.Services.DTOs
{
    public class CreateUserDto
    {
        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        [Phone]
        [MaxLength(50)]
        public string? Phone { get; set; }

        [Required]
        [MinLength(6)]
        [MaxLength(255)]
        public string Password { get; set; } = string.Empty;

        public int? RoleId { get; set; }

        [MaxLength(255)]
        public string? LocationAddress { get; set; }

        [MaxLength(100)]
        public string? LocationCity { get; set; }

        [MaxLength(255)]
        public string? AvatarUrl { get; set; }
    }
}