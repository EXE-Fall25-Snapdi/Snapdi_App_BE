using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snapdi.Services.DTOs
{
    /// <summary>
    /// DTO for updating photographer level (Admin only)
    /// </summary>
    public class UpdatePhotographerLevelDto
    {
        /// <summary>
        /// Photographer level - must be one of the predefined levels
        /// Available levels: "Beginner", "Intermediate", "Advanced", "Professional", "Expert"
        /// </summary>
        /// <example>Professional</example>
        [MaxLength(50, ErrorMessage = "Level photographer cannot exceed 50 characters")]
        public string LevelPhotographer { get; set; } = string.Empty;
    }
}
