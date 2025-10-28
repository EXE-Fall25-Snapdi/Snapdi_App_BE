using System.ComponentModel.DataAnnotations;

namespace Snapdi.Services.DTOs;

/// <summary>
/// Style DTO with Id and Name
/// </summary>
public class StyleDto
{
    public int StyleId { get; set; }
    public string StyleName { get; set; } = null!;
}

public class CreateStyleDto
{
    [Required(ErrorMessage = "Style name is required")]
    [StringLength(100, ErrorMessage = "Style name cannot exceed 100 characters")]
    public string StyleName { get; set; } = null!;
}

public class UpdateStyleDto
{
    [Required(ErrorMessage = "Style name is required")]
    [StringLength(100, ErrorMessage = "Style name cannot exceed 100 characters")]
    public string StyleName { get; set; } = null!;
}
