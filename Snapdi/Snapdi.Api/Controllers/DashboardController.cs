using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Snapdi.Services.DTOs;
using Snapdi.Services.Interfaces;
using System;
using System.Threading.Tasks;

namespace Snapdi.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "ADMIN")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(
            IDashboardService dashboardService,
            ILogger<DashboardController> logger)
        {
            _dashboardService = dashboardService;
            _logger = logger;
        }

        /// <summary>
        /// Get revenue data grouped by day for admin dashboard
        /// </summary>
        /// <param name="startDate">Optional start date (defaults to 30 days ago)</param>
        /// <param name="endDate">Optional end date (defaults to today)</param>
        /// <returns>Revenue data grouped by day</returns>
        [HttpGet("revenue-by-day")]
        public async Task<ActionResult<DashboardRevenueResponseDto>> GetRevenueByDay(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                // Default to last 30 days if no dates provided
                var end = endDate ?? DateTime.UtcNow;
                var start = startDate ?? end.AddDays(-30);

                // Validate date range
                if (start > end)
                {
                    return BadRequest(new
                    {
                        error = "Invalid date range",
                        message = "Start date must be before or equal to end date"
                    });
                }

                // Limit to maximum 1 year range to prevent performance issues
                if ((end - start).TotalDays > 365)
                {
                    return BadRequest(new
                    {
                        error = "Date range too large",
                        message = "Maximum date range is 365 days"
                    });
                }

                var result = await _dashboardService.GetRevenueByDayAsync(start, end);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving revenue data for dashboard");
                return StatusCode(500, new
                {
                    error = "Internal server error",
                    message = "An error occurred while retrieving revenue data",
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Get dashboard summary statistics for admin dashboard
        /// </summary>
        /// <returns>Complete dashboard statistics including user counts, revenue, and transactions</returns>
        [HttpGet("statistics")]
        public async Task<ActionResult<DashboardStatisticsDto>> GetDashboardStatistics()
        {
            try
            {
                var result = await _dashboardService.GetDashboardStatisticsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving dashboard statistics");
                return StatusCode(500, new
                {
                    error = "Internal server error",
                    message = "An error occurred while retrieving dashboard statistics",
                    details = ex.Message
                });
            }
        }
    }
}

