using Microsoft.EntityFrameworkCore;
using Snapdi.Repositories.Context;
using Snapdi.Repositories.Interfaces;
using Snapdi.Repositories.Models;

namespace Snapdi.Repositories.Repositories
{
    /// <summary>
    /// Repository for payment operations
    /// </summary>
    public class PaymentRepository : BaseRepository<Payment>, IPaymentRepository
    {
        private readonly new SnapdiDbV2Context _context;

        public PaymentRepository(SnapdiDbV2Context context) : base(context)
        {
            _context = context;
        }

        /// <summary>
        /// Search payments with advanced filtering and paging
        /// </summary>
        public async Task<(IEnumerable<Payment> Payments, int TotalCount)> SearchPaymentsAsync(
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
        )
        {
            var query = _context.Payments
                .Include(p => p.PaymentStatus)
                .Include(p => p.FeePolicy)
                .Include(p => p.Booking)
                    .ThenInclude(b => b!.Customer)
                .Include(p => p.Booking)
                    .ThenInclude(b => b!.Photographer)
                .Include(p => p.Booking)
                    .ThenInclude(b => b!.Status)
                .AsQueryable();

            // Apply enhanced search term filter
            if (!string.IsNullOrEmpty(searchTerm))
            {
                var searchLower = searchTerm.ToLower();
                query = query.Where(p =>
                    // Search in customer name
                    (p.Booking != null && p.Booking.Customer != null &&
                     (p.Booking.Customer.Name.ToLower().Contains(searchLower))) ||
                    //p.Booking.Customer.Email.ToLower().Contains(searchLower))) ||
                    // Search in photographer name
                    (p.Booking != null && p.Booking.Photographer != null &&
                     (p.Booking.Photographer.Name.ToLower().Contains(searchLower)))
                //p.Booking.Photographer.Email.ToLower().Contains(searchLower))) ||
                // Search in transaction reference
                //(p.TransactionReference != null && p.TransactionReference.ToLower().Contains(searchLower)) ||
                // Search in booking location
                //(p.Booking != null && p.Booking.LocationAddress != null && 
                // p.Booking.LocationAddress.ToLower().Contains(searchLower))
                );
            }

            if (!string.IsNullOrEmpty(paymentStatus))
            {
                var statusLower = paymentStatus.ToLower();
                query = query.Where(p => p.PaymentStatus != null &&
                                   p.PaymentStatus.StatusName.ToLower().Contains(statusLower));
            }

            if (!string.IsNullOrEmpty(bookingStatus))
            {
                var bookingStatusLower = bookingStatus.ToLower();
                query = query.Where(p => p.Booking != null && p.Booking.Status != null &&
                                   p.Booking.Status.StatusName.ToLower().Contains(bookingStatusLower));
            }

            if (!string.IsNullOrEmpty(transactionMethod))
            {
                var methodLower = transactionMethod.ToLower();
                query = query.Where(p => p.TransactionMethod != null &&
                                   p.TransactionMethod.ToLower().Contains(methodLower));
            }

            if (minAmount.HasValue)
            {
                query = query.Where(p => p.Amount >= minAmount.Value);
            }

            if (maxAmount.HasValue)
            {
                query = query.Where(p => p.Amount <= maxAmount.Value);
            }

            if (feePolicyId.HasValue)
            {
                query = query.Where(p => p.FeePolicyId == feePolicyId.Value);
            }

            if (paymentDateFrom.HasValue)
            {
                query = query.Where(p => p.PaymentDate >= paymentDateFrom.Value);
            }

            if (paymentDateTo.HasValue)
            {
                query = query.Where(p => p.PaymentDate <= paymentDateTo.Value);
            }

            if (bookingDateFrom.HasValue)
            {
                query = query.Where(p => p.Booking != null && p.Booking.ScheduleAt >= bookingDateFrom.Value);
            }

            if (bookingDateTo.HasValue)
            {
                query = query.Where(p => p.Booking != null && p.Booking.ScheduleAt <= bookingDateTo.Value);
            }

            if (!string.IsNullOrEmpty(bookingLocation))
            {
                var locationLower = bookingLocation.ToLower();
                query = query.Where(p => p.Booking != null && p.Booking.LocationAddress != null &&
                                   p.Booking.LocationAddress.ToLower().Contains(locationLower));
            }

            // Get total count before paging
            var totalCount = await query.CountAsync();

            // Apply sorting
            query = sortBy?.ToLower() switch
            {
                "amount" => sortDirection?.ToLower() == "asc"
                    ? query.OrderBy(p => p.Amount)
                    : query.OrderByDescending(p => p.Amount),
                "customername" => sortDirection?.ToLower() == "asc"
                    ? query.OrderBy(p => p.Booking != null && p.Booking.Customer != null ? p.Booking.Customer.Name : "")
                    : query.OrderByDescending(p => p.Booking != null && p.Booking.Customer != null ? p.Booking.Customer.Name : ""),
                "photographername" => sortDirection?.ToLower() == "asc"
                    ? query.OrderBy(p => p.Booking != null && p.Booking.Photographer != null ? p.Booking.Photographer.Name : "")
                    : query.OrderByDescending(p => p.Booking != null && p.Booking.Photographer != null ? p.Booking.Photographer.Name : ""),
                "bookingdate" => sortDirection?.ToLower() == "asc"
                    ? query.OrderBy(p => p.Booking != null ? p.Booking.ScheduleAt : DateTime.MinValue)
                    : query.OrderByDescending(p => p.Booking != null ? p.Booking.ScheduleAt : DateTime.MinValue),
                "status" => sortDirection?.ToLower() == "asc"
                    ? query.OrderBy(p => p.PaymentStatus != null ? p.PaymentStatus.StatusName : "")
                    : query.OrderByDescending(p => p.PaymentStatus != null ? p.PaymentStatus.StatusName : ""),
                _ => sortDirection?.ToLower() == "asc"
                    ? query.OrderBy(p => p.PaymentDate)
                    : query.OrderByDescending(p => p.PaymentDate)
            };

            // Apply paging
            var payments = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (payments, totalCount);
        }

        /// <summary>
        /// Get payments with summary statistics for search criteria
        /// </summary>
        public async Task<(double TotalAmount, double TotalFeeAmount, double TotalNetAmount,
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
        )
        {
            var query = _context.Payments
                .Include(p => p.PaymentStatus)
                .Include(p => p.Booking)
                    .ThenInclude(b => b!.Customer)
                .Include(p => p.Booking)
                    .ThenInclude(b => b!.Photographer)
                .Include(p => p.Booking)
                    .ThenInclude(b => b!.Status)
                .AsQueryable();

            // Apply enhanced search term filter (same as search method)
            if (!string.IsNullOrEmpty(searchTerm))
            {
                var searchLower = searchTerm.ToLower();
                query = query.Where(p =>
                    // Search in customer name and email
                    (p.Booking != null && p.Booking.Customer != null &&
                     (p.Booking.Customer.Name.ToLower().Contains(searchLower))) ||
                    //p.Booking.Customer.Email.ToLower().Contains(searchLower))) ||
                    // Search in photographer name and email
                    (p.Booking != null && p.Booking.Photographer != null &&
                     (p.Booking.Photographer.Name.ToLower().Contains(searchLower)))
                //p.Booking.Photographer.Email.ToLower().Contains(searchLower))) ||
                // Search in transaction reference
                //(p.TransactionReference != null && p.TransactionReference.ToLower().Contains(searchLower)) ||
                // Search in booking location
                //(p.Booking != null && p.Booking.LocationAddress != null && 
                // p.Booking.LocationAddress.ToLower().Contains(searchLower))
                );
            }

            if (!string.IsNullOrEmpty(paymentStatus))
            {
                var statusLower = paymentStatus.ToLower();
                query = query.Where(p => p.PaymentStatus != null &&
                                   p.PaymentStatus.StatusName.ToLower().Contains(statusLower));
            }

            if (!string.IsNullOrEmpty(bookingStatus))
            {
                var bookingStatusLower = bookingStatus.ToLower();
                query = query.Where(p => p.Booking != null && p.Booking.Status != null &&
                                   p.Booking.Status.StatusName.ToLower().Contains(bookingStatusLower));
            }

            if (!string.IsNullOrEmpty(transactionMethod))
            {
                var methodLower = transactionMethod.ToLower();
                query = query.Where(p => p.TransactionMethod != null &&
                                   p.TransactionMethod.ToLower().Contains(methodLower));
            }

            if (minAmount.HasValue)
            {
                query = query.Where(p => p.Amount >= minAmount.Value);
            }

            if (maxAmount.HasValue)
            {
                query = query.Where(p => p.Amount <= maxAmount.Value);
            }

            if (feePolicyId.HasValue)
            {
                query = query.Where(p => p.FeePolicyId == feePolicyId.Value);
            }

            if (paymentDateFrom.HasValue)
            {
                query = query.Where(p => p.PaymentDate >= paymentDateFrom.Value);
            }

            if (paymentDateTo.HasValue)
            {
                query = query.Where(p => p.PaymentDate <= paymentDateTo.Value);
            }

            if (bookingDateFrom.HasValue)
            {
                query = query.Where(p => p.Booking != null && p.Booking.ScheduleAt >= bookingDateFrom.Value);
            }

            if (bookingDateTo.HasValue)
            {
                query = query.Where(p => p.Booking != null && p.Booking.ScheduleAt <= bookingDateTo.Value);
            }

            if (!string.IsNullOrEmpty(bookingLocation))
            {
                var locationLower = bookingLocation.ToLower();
                query = query.Where(p => p.Booking != null && p.Booking.LocationAddress != null &&
                                   p.Booking.LocationAddress.ToLower().Contains(locationLower));
            }

            // Calculate summary statistics
            var payments = await query.ToListAsync();

            var totalAmount = payments.Sum(p => p.Amount);
            var totalFeeAmount = payments.Sum(p => p.FeeAmount ?? 0);
            var totalNetAmount = payments.Sum(p => p.NetAmount ?? 0);

            var confirmedCount = payments.Count(p => p.PaymentStatus != null &&
                                               p.PaymentStatus.StatusName.ToLower().Contains("confirmed"));
            var paidCount = payments.Count(p => p.PaymentStatus != null &&
                                          (p.PaymentStatus.StatusName.ToLower().Contains("paid") ||
                                           p.PaymentStatus.StatusName.ToLower().Contains("done") ||
                                           p.PaymentStatus.StatusName.ToLower().Contains("completed")));
            var pendingCount = payments.Count(p => p.PaymentStatus != null &&
                                             p.PaymentStatus.StatusName.ToLower().Contains("pending"));

            return (totalAmount, totalFeeAmount, totalNetAmount, confirmedCount, paidCount, pendingCount);
        }

        /// <summary>
        /// Get payment by ID with all related data
        /// </summary>
        public async Task<Payment?> GetPaymentWithDetailsAsync(int paymentId)
        {
            return await _context.Payments
                .Include(p => p.PaymentStatus)
                .Include(p => p.FeePolicy)
                .Include(p => p.Booking)
                    .ThenInclude(b => b!.Customer)
                .Include(p => p.Booking)
                    .ThenInclude(b => b!.Photographer)
                .Include(p => p.Booking)
                    .ThenInclude(b => b!.Status)
                .FirstOrDefaultAsync(p => p.PaymentId == paymentId);
        }

        /// <summary>
        /// Get all payments for a specific booking
        /// </summary>
        public async Task<IEnumerable<Payment>> GetPaymentsByBookingIdAsync(int bookingId)
        {
            return await _context.Payments
                .Include(p => p.PaymentStatus)
                .Include(p => p.FeePolicy)
                .Where(p => p.BookingId == bookingId)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }

        /// <summary>
        /// Get payments by customer ID with paging
        /// </summary>
        public async Task<(IEnumerable<Payment> Payments, int TotalCount)> GetPaymentsByCustomerIdAsync(
            int customerId, int page, int pageSize)
        {
            var query = _context.Payments
                .Include(p => p.PaymentStatus)
                .Include(p => p.FeePolicy)
                .Include(p => p.Booking)
                    .ThenInclude(b => b!.Customer)
                .Include(p => p.Booking)
                    .ThenInclude(b => b!.Photographer)
                .Include(p => p.Booking)
                    .ThenInclude(b => b!.Status)
                .Where(p => p.Booking != null && p.Booking.CustomerId == customerId);

            var totalCount = await query.CountAsync();

            var payments = await query
                .OrderByDescending(p => p.PaymentDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (payments, totalCount);
        }

        /// <summary>
        /// Get payments by photographer ID with paging
        /// </summary>
        public async Task<(IEnumerable<Payment> Payments, int TotalCount)> GetPaymentsByPhotographerIdAsync(
            int photographerId, int page, int pageSize)
        {
            var query = _context.Payments
                .Include(p => p.PaymentStatus)
                .Include(p => p.FeePolicy)
                .Include(p => p.Booking)
                    .ThenInclude(b => b!.Customer)
                .Include(p => p.Booking)
                    .ThenInclude(b => b!.Photographer)
                .Include(p => p.Booking)
                    .ThenInclude(b => b!.Status)
                .Where(p => p.Booking != null && p.Booking.PhotographerId == photographerId);

            var totalCount = await query.CountAsync();

            var payments = await query
                .OrderByDescending(p => p.PaymentDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (payments, totalCount);
        }
    }
}
