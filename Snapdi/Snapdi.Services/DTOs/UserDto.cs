namespace Snapdi.Services.DTOs
{
    public class UserDto
    {
        public int UserId { get; set; }
        public int? RoleId { get; set; }
        public string? RoleName { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public bool IsActive { get; set; }
        public bool IsVerify { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? LocationAddress { get; set; }
        public string? LocationCity { get; set; }
        public string? AvatarUrl { get; set; }
    }
}