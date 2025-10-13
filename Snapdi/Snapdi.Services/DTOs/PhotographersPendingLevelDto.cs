namespace Snapdi.Services.DTOs
{
    /// <summary>
    /// Response DTO for photographers pending level assignment, grouped by portfolio status
    /// </summary>
    public class PhotograhpersPendingLevelResponseDto
    {
        /// <summary>
        /// Photographers who have portfolio photos uploaded
        /// </summary>
        public List<UserWithPhotographerDto> WithPortfolio { get; set; } = new();

        /// <summary>
        /// Photographers who don't have any portfolio photos yet
        /// </summary>
        public List<UserWithPhotographerDto> WithoutPortfolio { get; set; } = new();

        /// <summary>
        /// Total count of photographers pending level assignment
        /// </summary>
        public int TotalCount => WithPortfolio.Count + WithoutPortfolio.Count;

        /// <summary>
        /// Count of photographers with portfolio
        /// </summary>
        public int WithPortfolioCount => WithPortfolio.Count;

        /// <summary>
        /// Count of photographers without portfolio
        /// </summary>
        public int WithoutPortfolioCount => WithoutPortfolio.Count;

        /// <summary>
        /// Percentage of photographers with portfolio
        /// </summary>
        public double WithPortfolioPercentage => TotalCount > 0 ? Math.Round((double)WithPortfolioCount / TotalCount * 100, 1) : 0;

        /// <summary>
        /// Percentage of photographers without portfolio
        /// </summary>
        public double WithoutPortfolioPercentage => TotalCount > 0 ? Math.Round((double)WithoutPortfolioCount / TotalCount * 100, 1) : 0;

        /// <summary>
        /// Summary message for admin
        /// </summary>
        public string Summary => TotalCount == 0 
            ? "No photographers are currently pending level assignment."
            : $"Found {TotalCount} photographer(s) pending level assignment: {WithPortfolioCount} with portfolio ({WithPortfolioPercentage}%), {WithoutPortfolioCount} without portfolio ({WithoutPortfolioPercentage}%).";

        /// <summary>
        /// Recommendation for admin action
        /// </summary>
        public string Recommendation => TotalCount == 0 
            ? "No action needed at this time."
            : WithPortfolioCount > 0 
                ? "Recommend reviewing photographers with portfolios first as they are more ready for level assignment."
                : "All pending photographers need to upload portfolios before level assignment.";
    }
}