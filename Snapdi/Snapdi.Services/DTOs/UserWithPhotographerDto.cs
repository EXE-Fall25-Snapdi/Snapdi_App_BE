namespace Snapdi.Services.DTOs
{
    public class UserWithPhotographerDto : UserDto
    {
        public PhotographerProfileDto? PhotographerProfile { get; set; }
    }

    public class PhotographerProfileDto
    {
        public int UserId { get; set; }
        public int? EquipmentId { get; set; }
        public double? AvgRating { get; set; }
        public bool IsAvailable { get; set; }
        public string? Description { get; set; }
        public int? PhotoPortfolioId { get; set; }
        public PhotoEquipmentDto? Equipment { get; set; }
        public PhotoPortfolioDto? PhotoPortfolio { get; set; }
    }

    public class PhotoEquipmentDto
    {
        public int EquipmentId { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class PhotoPortfolioDto
    {
        public int PhotoPortfolioId { get; set; }
        public string PhotoUrl { get; set; } = string.Empty;
    }
}