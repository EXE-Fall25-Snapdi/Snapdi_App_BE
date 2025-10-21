using System.ComponentModel.DataAnnotations;

namespace Snapdi.Services.DTOs;

public class CreatePhotoTypeDto
{
    [Required(ErrorMessage = "Photo type name is required")]
    [StringLength(100, ErrorMessage = "Photo type name cannot exceed 100 characters")]
    public string PhotoTypeName { get; set; } = null!;
}

public class UpdatePhotoTypeDto
{
    [Required(ErrorMessage = "Photo type name is required")]
    [StringLength(100, ErrorMessage = "Photo type name cannot exceed 100 characters")]
    public string PhotoTypeName { get; set; } = null!;
}
