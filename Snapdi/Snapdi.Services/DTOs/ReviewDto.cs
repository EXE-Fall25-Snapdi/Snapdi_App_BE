using System.ComponentModel.DataAnnotations;

namespace Snapdi.Services.DTOs
{
    /// <summary>
    /// DTO for Review display
    /// </summary>
    public class ReviewDto
    {
        public int ReviewId { get; set; }
        public int? BookingId { get; set; }
        public int? FromUserId { get; set; }
        public string? FromUserName { get; set; }
        public string? FromUserAvatar { get; set; }
        public double Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreateAt { get; set; }
    }

    /// <summary>
    /// DTO for creating a review
    /// </summary>
    public class CreateReviewDto
    {
        [Required(ErrorMessage = "BookingId is required")]
        public int BookingId { get; set; }

        [Required(ErrorMessage = "Rating is required")]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public double Rating { get; set; }

        [MaxLength(1000, ErrorMessage = "Comment cannot exceed 1000 characters")]
        public string? Comment { get; set; }
    }

    /// <summary>
    /// DTO for updating a review
    /// </summary>
    public class UpdateReviewDto
    {
        [Required(ErrorMessage = "Rating is required")]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public double Rating { get; set; }

        [MaxLength(1000, ErrorMessage = "Comment cannot exceed 1000 characters")]
        public string? Comment { get; set; }
    }
}
