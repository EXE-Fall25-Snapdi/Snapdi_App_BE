using System;
using System.Collections.Generic;

namespace Snapdi.Services.DTOs
{
    /// <summary>
    /// Represents daily revenue data
    /// </summary>
    public class DailyRevenueDto
    {
        public DateTime Date { get; set; }
        public double TotalRevenue { get; set; }
        public int TransactionCount { get; set; }
    }

    /// <summary>
    /// Request DTO for dashboard revenue queries
    /// </summary>
    public class DashboardRevenueRequestDto
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    /// <summary>
    /// Response DTO for dashboard revenue data
    /// </summary>
    public class DashboardRevenueResponseDto
    {
        public List<DailyRevenueDto> DailyRevenue { get; set; } = new List<DailyRevenueDto>();
        public double TotalRevenue { get; set; }
        public double AverageDailyRevenue { get; set; }
        public int TotalTransactions { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    /// <summary>
    /// User statistics for dashboard
    /// </summary>
    public class UserStatisticsDto
    {
        public int TotalUsers { get; set; }
        public int TotalAdmins { get; set; }
        public int TotalPhotographers { get; set; }
        public int TotalCustomers { get; set; }
    }

    /// <summary>
    /// Revenue statistics for dashboard
    /// </summary>
    public class RevenueStatisticsDto
    {
        public double TodayRevenue { get; set; }
        public double TotalRevenue { get; set; }
    }

    /// <summary>
    /// Transaction statistics for dashboard
    /// </summary>
    public class TransactionStatisticsDto
    {
        public int TodayTransactions { get; set; }
        public int TotalTransactions { get; set; }
    }

    /// <summary>
    /// DTO for review statistics
    /// </summary>
    public class ReviewStatisticsDto
    {
        /// <summary>
        /// Total number of reviews
        /// </summary>
        public int TotalReviews { get; set; }

        /// <summary>
        /// Average rating (0.0 to 5.0)
        /// </summary>
        public double AverageRating { get; set; }

        /// <summary>
        /// Number of 5-star reviews
        /// </summary>
        public int FiveStarCount { get; set; }

        /// <summary>
        /// Number of 4-star reviews
        /// </summary>
        public int FourStarCount { get; set; }

        /// <summary>
        /// Number of 3-star reviews
        /// </summary>
        public int ThreeStarCount { get; set; }

        /// <summary>
        /// Number of 2-star reviews
        /// </summary>
        public int TwoStarCount { get; set; }

        /// <summary>
        /// Number of 1-star reviews
        /// </summary>
        public int OneStarCount { get; set; }
    }

    /// <summary>
    /// Complete dashboard statistics
    /// </summary>
    public class DashboardStatisticsDto
    {
        public UserStatisticsDto UserStatistics { get; set; } = new UserStatisticsDto();
        public RevenueStatisticsDto RevenueStatistics { get; set; } = new RevenueStatisticsDto();
        public TransactionStatisticsDto TransactionStatistics { get; set; } = new TransactionStatisticsDto();
        public ReviewStatisticsDto ReviewStatistics { get; set; } = new ReviewStatisticsDto();
    }
}

