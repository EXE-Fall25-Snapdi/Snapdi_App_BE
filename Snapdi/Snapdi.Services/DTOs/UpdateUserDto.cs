using System.ComponentModel.DataAnnotations;

namespace Snapdi.Services.DTOs
{
    public class UpdateUserDto
    {
        [MaxLength(255)]
        public string? Name { get; set; }

        [Phone]
        [MaxLength(50)]
        public string? Phone { get; set; }

        [MaxLength(255)]
        public string? LocationAddress { get; set; }

        [MaxLength(100)]
        public string? LocationCity { get; set; }

        [MaxLength(255)]
        public string? AvatarUrl { get; set; }

        public bool? IsActive { get; set; }
        public bool? IsVerify { get; set; }
    }
}