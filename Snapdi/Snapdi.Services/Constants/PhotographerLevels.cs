namespace Snapdi.Services.Constants
{
    /// <summary>
    /// Constants for photographer levels used in the system
    /// </summary>
    public static class PhotographerLevels
    {
        public const string Beginner = "Beginner";
        public const string Intermediate = "Intermediate"; 
        public const string Advanced = "Advanced";
        public const string Professional = "Professional";
        public const string Expert = "Expert";

        /// <summary>
        /// All available photographer levels
        /// </summary>
        public static readonly string[] AllLevels = {
            Beginner,
            Intermediate,
            Advanced,
            Professional,
            Expert
        };

        /// <summary>
        /// Check if a level is valid
        /// </summary>
        /// <param name="level">Level to validate</param>
        /// <returns>True if level is valid</returns>
        public static bool IsValidLevel(string level)
        {
            return AllLevels.Contains(level, StringComparer.OrdinalIgnoreCase);
        }
    }
}