using System.ComponentModel.DataAnnotations;

namespace Snapdi.Services.DTOs;

/// <summary>
/// Photo type DTO with Id and Name
/// </summary>
public class PhotoTypeDto
{
    public int PhotoTypeId { get; set; }
    public string PhotoTypeName { get; set; } = null!;
}

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

/// <summary>
/// Photo type with photographer-specific pricing and time
/// Used when displaying photographer's photo type offerings
/// </summary>
public class PhotoTypeWithPricingDto
{
    public int PhotoTypeId { get; set; }
    public double? PhotoPrice { get; set; }
    public int? Time { get; set; }
}

public class PhotoTypeWithPricingResponseDto
{
    public int PhotoTypeId { get; set; }
    public string PhotoTypeName { get; set; } = null!;
    public double? PhotoPrice { get; set; }
    public int? Time { get; set; }
}
