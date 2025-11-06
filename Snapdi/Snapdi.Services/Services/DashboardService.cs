using Snapdi.Repositories.Interfaces;
using Snapdi.Services.DTOs;
using Snapdi.Services.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Snapdi.Services.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IUserRepository _userRepository;
        private readonly IBookingRepository _bookingRepository;

        public DashboardService(
            IPaymentRepository paymentRepository,
            IUserRepository userRepository,
            IBookingRepository bookingRepository)
        {
            _paymentRepository = paymentRepository;
            _userRepository = userRepository;
            _bookingRepository = bookingRepository;
        }

        /// <summary>
        /// Get revenue data grouped by day for a specific date range
        /// </summary>
        public async Task<DashboardRevenueResponseDto> GetRevenueByDayAsync(DateTime startDate, DateTime endDate)
        {
            // Convert to UTC and ensure start date is at beginning of day and end date is at end of day
            startDate = DateTime.SpecifyKind(startDate.Date, DateTimeKind.Utc);
            endDate = DateTime.SpecifyKind(endDate.Date.AddDays(1).AddSeconds(-1), DateTimeKind.Utc);

            // Get all payments in the date range
            var payments = await _paymentRepository.GetPaymentsByDateRangeAsync(startDate, endDate);

            // Group by date and calculate daily revenue
            var dailyRevenue = payments
                .GroupBy(p => p.PaymentDate.Date)
                .Select(g => new DailyRevenueDto
                {
                    Date = g.Key,
                    TotalRevenue = g.Sum(p => p.Amount),
                    TransactionCount = g.Count()
                })
                .OrderBy(d => d.Date)
                .ToList();

            // Calculate totals and averages
            var totalRevenue = dailyRevenue.Sum(d => d.TotalRevenue);
            var totalTransactions = dailyRevenue.Sum(d => d.TransactionCount);
            var dayCount = (endDate.Date - startDate.Date).Days + 1;
            var averageDailyRevenue = dayCount > 0 ? totalRevenue / dayCount : 0;

            return new DashboardRevenueResponseDto
            {
                DailyRevenue = dailyRevenue,
                TotalRevenue = totalRevenue,
                AverageDailyRevenue = Math.Round(averageDailyRevenue, 2),
                TotalTransactions = totalTransactions,
                StartDate = startDate.Date,
                EndDate = endDate.Date
            };
        }

        /// <summary>
        /// Get complete dashboard statistics including user counts, revenue, and transactions
        /// </summary>
        public async Task<DashboardStatisticsDto> GetDashboardStatisticsAsync()
        {
            var today = DateTime.UtcNow.Date;
            var endOfToday = today.AddDays(1).AddSeconds(-1);

            // Get user statistics
            var totalUsers = await _userRepository.GetUserCountByRoleAsync();
            var totalAdmin = await _userRepository.GetUserCountByRoleAsync(1); //RoleId 1 = admin
            var totalPhotographers = await _userRepository.GetUserCountByRoleAsync(3); // RoleId 3 = Photographer
            var totalCustomers = await _userRepository.GetUserCountByRoleAsync(2); // RoleId 2 = Customer

            // Get today's revenue and transactions from payments
            var todayPayments = await _paymentRepository.GetPaymentsByDateRangeAsync(today, endOfToday);
            var todayRevenue = todayPayments.Sum(p => p.Amount);
            var todayTransactions = todayPayments.Count();

            // Get total revenue and transactions from all time
            // Use a reasonable start date (e.g., 10 years ago) instead of DateTime.MinValue
            // This avoids PostgreSQL/SQL Server DateTime range issues
            var allTimeStartDate = DateTime.SpecifyKind(new DateTime(2000, 1, 1), DateTimeKind.Utc);
            var allTimeEndDate = DateTime.SpecifyKind(DateTime.UtcNow.Date.AddYears(1), DateTimeKind.Utc); // Include future payments
            
            var allPayments = await _paymentRepository.GetPaymentsByDateRangeAsync(allTimeStartDate, allTimeEndDate);
            var totalRevenue = allPayments.Sum(p => p.Amount);
            var totalTransactions = allPayments.Count();

            return new DashboardStatisticsDto
            {
                UserStatistics = new UserStatisticsDto
                {
                    TotalUsers = totalUsers,
                    TotalAdmins = totalAdmin,
                    TotalPhotographers = totalPhotographers,
                    TotalCustomers = totalCustomers
                },
                RevenueStatistics = new RevenueStatisticsDto
                {
                    TodayRevenue = Math.Round(todayRevenue, 2),
                    TotalRevenue = Math.Round(totalRevenue, 2)
                },
                TransactionStatistics = new TransactionStatisticsDto
                {
                    TodayTransactions = todayTransactions,
                    TotalTransactions = totalTransactions
                }
            };
        }
    }
}

