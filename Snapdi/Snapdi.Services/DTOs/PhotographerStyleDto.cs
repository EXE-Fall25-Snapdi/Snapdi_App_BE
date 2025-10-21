using System.ComponentModel.DataAnnotations;

namespace Snapdi.Services.DTOs;

public class PhotographerStyleDto
{
    public int UserId { get; set; }
    public int StyleId { get; set; }
    public string StyleName { get; set; } = null!;
    public string Name { get; set; } = null!;
}

public class CreatePhotographerStyleDto
{
    [Required(ErrorMessage = "User ID is required")]
    public int UserId { get; set; }

    [Required(ErrorMessage = "Style ID is required")]
    public int StyleId { get; set; }
}

public class PhotographerStyleResponseDto
{
    public int UserId { get; set; }
    public string Name { get; set; } = null!;
    public List<StyleDto> SelectedStyles { get; set; } = new List<StyleDto>();
}

public class StyleSelectionDto
{
    public int StyleId { get; set; }
    public string StyleName { get; set; } = null!;
    public bool IsSelected { get; set; }
}

/// <summary>
/// DTO for adding multiple styles to photographer
/// </summary>
public class AddMultipleStylesDto
{
    /// <summary>
    /// List of style IDs to add to photographer
    /// </summary>
    [Required(ErrorMessage = "Style IDs are required")]
    [MinLength(1, ErrorMessage = "At least one style ID is required")]
    public List<int> StyleIds { get; set; } = new List<int>();
}

/// <summary>
/// Response DTO for multiple style operations
/// </summary>
public class MultipleStyleOperationResponseDto
{
    /// <summary>
    /// Number of styles successfully processed
    /// </summary>
    public int SuccessCount { get; set; }

    /// <summary>
    /// Number of styles that failed to process
    /// </summary>
    public int FailedCount { get; set; }

    /// <summary>
    /// Total number of styles attempted
    /// </summary>
    public int TotalAttempted { get; set; }

    /// <summary>
    /// List of style IDs that failed to process
    /// </summary>
    public List<int> FailedStyleIds { get; set; } = new List<int>();

    /// <summary>
    /// List of style IDs that were successfully processed
    /// </summary>
    public List<int> SuccessfulStyleIds { get; set; } = new List<int>();

    /// <summary>
    /// Overall success status
    /// </summary>
    public bool IsCompleteSuccess => FailedCount == 0;

    /// <summary>
    /// Operation result message
    /// </summary>
    public string Message { get; set; } = string.Empty;
}
