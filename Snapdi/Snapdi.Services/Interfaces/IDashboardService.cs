using Snapdi.Services.DTOs;
using System;
using System.Threading.Tasks;

namespace Snapdi.Services.Interfaces
{
    public interface IDashboardService
    {
        /// <summary>
        /// Get revenue data grouped by day for a specific date range
        /// </summary>
        /// <param name="startDate">Start date of the range</param>
        /// <param name="endDate">End date of the range</param>
        /// <returns>Dashboard revenue response with daily breakdown</returns>
        Task<DashboardRevenueResponseDto> GetRevenueByDayAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Get complete dashboard statistics including user counts, revenue, and transactions
        /// </summary>
        /// <returns>Dashboard statistics</returns>
        Task<DashboardStatisticsDto> GetDashboardStatisticsAsync();
    }
}

