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
            // Ensure start date is at beginning of day and end date is at end of day
            startDate = startDate.Date;
            endDate = endDate.Date.AddDays(1).AddSeconds(-1);

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

            // Get user statistics
            var totalUsers = await _userRepository.GetUserCountByRoleAsync();
            var totalAdmin = await _userRepository.GetUserCountByRoleAsync(1); //RoleId 1 = admin
            var totalPhotographers = await _userRepository.GetUserCountByRoleAsync(3); // RoleId 2 = Photographer
            var totalCustomers = await _userRepository.GetUserCountByRoleAsync(2); // RoleId 3 = Customer

            // Get revenue statistics
            var totalRevenue = await _bookingRepository.GetTotalRevenueAsync();
            var todayCompletedBookings = await _bookingRepository.GetCompletedBookingsCountAsync(today);
            
            // Calculate today's revenue by getting completed bookings for today
            var todayRevenue = 0.0;
            // Note: We would need a method to get today's revenue specifically
            // For now, we'll estimate based on average if needed

            // Get transaction statistics
            var totalTransactions = await _bookingRepository.GetCompletedBookingsCountAsync();
            var todayTransactions = todayCompletedBookings;

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

