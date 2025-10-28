using Snapdi.Services.DTOs;

namespace Snapdi.Services.Interfaces
{
    /// <summary>
    /// Interface for payment service operations
    /// </summary>
    public interface IPaymentsService
    {
        /// <summary>
        /// Search payments with filtering and paging
        /// </summary>
        /// <param name="searchDto">Search criteria and pagination parameters</param>
        /// <returns>Paginated search results</returns>
        Task<PaymentSearchResultDto> SearchPaymentsAsync(PaymentSearchDto searchDto);

        /// <summary>
        /// Get payment search summary statistics
        /// </summary>
        /// <param name="searchDto">Search criteria</param>
        /// <returns>Summary statistics for the search results</returns>
        Task<PaymentSearchSummaryDto> GetPaymentSearchSummaryAsync(PaymentSearchDto searchDto);

        /// <summary>
        /// Get payment by ID with full details
        /// </summary>
        /// <param name="paymentId">Payment ID</param>
        /// <returns>Payment details or null if not found</returns>
        Task<PaymentDto?> GetPaymentByIdAsync(int paymentId);

        /// <summary>
        /// Get all payments for a specific booking
        /// </summary>
        /// <param name="bookingId">Booking ID</param>
        /// <returns>List of payments for the booking</returns>
        Task<IEnumerable<PaymentDto>> GetPaymentsByBookingIdAsync(int bookingId);

        /// <summary>
        /// Get payments by customer ID
        /// </summary>
        /// <param name="customerId">Customer user ID</param>
        /// <param name="page">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Paginated payments for the customer</returns>
        Task<PaymentSearchResultDto> GetPaymentsByCustomerIdAsync(int customerId, int page = 1, int pageSize = 10);

        /// <summary>
        /// Get payments by photographer ID
        /// </summary>
        /// <param name="photographerId">Photographer user ID</param>
        /// <param name="page">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Paginated payments for the photographer</returns>
        Task<PaymentSearchResultDto> GetPaymentsByPhotographerIdAsync(int photographerId, int page = 1, int pageSize = 10);
    }
}
