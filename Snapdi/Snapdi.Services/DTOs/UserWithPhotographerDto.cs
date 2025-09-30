namespace Snapdi.Services.DTOs
{
    public class UserWithPhotographerDto : UserDto
    {
        public PhotographerProfileDto? PhotographerProfile { get; set; }
        public List<PhotoPortfolioDto>? PhotoPortfolios { get; set; }
    }

    public class PhotographerProfileDto
    {
        public int UserId { get; set; }
        public string? EquipmentDescription { get; set; }
        public string? YearsOfExperience { get; set; }
        public double? AvgRating { get; set; }
        public bool IsAvailable { get; set; }
        public string? Description { get; set; }
    }

    public class PhotoPortfolioDto
    {
        public int PhotoPortfolioId { get; set; }
        public int UserId { get; set; }
        public string PhotoUrl { get; set; } = string.Empty;
    }

    public class CreatePhotographerProfileDto
    {
        public string? EquipmentDescription { get; set; }
        public string? YearsOfExperience { get; set; }
        public bool IsAvailable { get; set; } = true;
        public string? Description { get; set; }
    }

    public class UpdatePhotographerProfileDto
    {
        public string? EquipmentDescription { get; set; }
        public string? YearsOfExperience { get; set; }
        public double? AvgRating { get; set; }
        public bool? IsAvailable { get; set; }
        public string? Description { get; set; }
    }

    public class CreatePhotoPortfolioDto
    {
        public string PhotoUrl { get; set; } = string.Empty;
    }

    public class UpdatePhotoPortfolioDto
    {
        public string? PhotoUrl { get; set; }
    }
}