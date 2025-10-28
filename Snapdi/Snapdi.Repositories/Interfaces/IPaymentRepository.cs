using Snapdi.Repositories.Models;

namespace Snapdi.Repositories.Interfaces
{
    /// <summary>
    /// Interface for payment repository operations
    /// </summary>
    public interface IPaymentRepository : IBaseRepository<Payment>
    {
        /// <summary>
        /// Search payments with advanced filtering and paging
        /// </summary>
        Task<(IEnumerable<Payment> Payments, int TotalCount)> SearchPaymentsAsync(
            int page,
            int pageSize,
            string? searchTerm = null,
            string? paymentStatus = null,
            string? bookingStatus = null,
            string? transactionMethod = null,
            double? minAmount = null,
            double? maxAmount = null,
            int? feePolicyId = null,
            DateTime? paymentDateFrom = null,
            DateTime? paymentDateTo = null,
            DateTime? bookingDateFrom = null,
            DateTime? bookingDateTo = null,
            string? bookingLocation = null,
            string? sortBy = "paymentDate",
            string? sortDirection = "desc"
        );

        /// <summary>
        /// Get payments with summary statistics for search criteria
        /// </summary>
        Task<(double TotalAmount, double TotalFeeAmount, double TotalNetAmount, 
               int ConfirmedCount, int PaidCount, int PendingCount)> GetPaymentSummaryAsync(
            string? searchTerm = null,
            string? paymentStatus = null,
            string? bookingStatus = null,
            string? transactionMethod = null,
            double? minAmount = null,
            double? maxAmount = null,
            int? feePolicyId = null,
            DateTime? paymentDateFrom = null,
            DateTime? paymentDateTo = null,
            DateTime? bookingDateFrom = null,
            DateTime? bookingDateTo = null,
            string? bookingLocation = null
        );

        /// <summary>
        /// Get payment by ID with all related data
        /// </summary>
        Task<Payment?> GetPaymentWithDetailsAsync(int paymentId);

        /// <summary>
        /// Get all payments for a specific booking
        /// </summary>
        Task<IEnumerable<Payment>> GetPaymentsByBookingIdAsync(int bookingId);

        /// <summary>
        /// Get payments by customer ID with paging
        /// </summary>
        Task<(IEnumerable<Payment> Payments, int TotalCount)> GetPaymentsByCustomerIdAsync(
            int customerId, int page, int pageSize);

        /// <summary>
        /// Get payments by photographer ID with paging
        /// </summary>
        Task<(IEnumerable<Payment> Payments, int TotalCount)> GetPaymentsByPhotographerIdAsync(
            int photographerId, int page, int pageSize);
    }
}