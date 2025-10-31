using Snapdi.Repositories.Interfaces;
using Snapdi.Repositories.Models;
using Snapdi.Services.DTOs;
using Snapdi.Services.Interfaces;

namespace Snapdi.Services.Services
{
    /// <summary>
    /// Service for payment operations
    /// </summary>
    public class PaymentService : IPaymentsService
    {
        private readonly IPaymentRepository _paymentRepository;

        public PaymentService(IPaymentRepository paymentRepository)
        {
            _paymentRepository = paymentRepository;
        }

        /// <summary>
        /// Search payments with filtering and paging
        /// </summary>
        public async Task<PaymentSearchResultDto> SearchPaymentsAsync(PaymentSearchDto searchDto)
        {
            var (payments, totalCount) = await _paymentRepository.SearchPaymentsAsync(
                searchDto.PageNumber,
                searchDto.PageSize,
                searchDto.SearchTerm,
                searchDto.PaymentStatus,
                searchDto.BookingStatus,
                searchDto.TransactionMethod,
                searchDto.MinAmount,
                searchDto.MaxAmount,
                searchDto.FeePolicyId,
                searchDto.PaymentDateFrom,
                searchDto.PaymentDateTo,
                searchDto.BookingDateFrom,
                searchDto.BookingDateTo,
                searchDto.BookingLocation,
                searchDto.SortBy,
                searchDto.SortDirection
            );

            var paymentDtos = payments.Select(MapToPaymentDto).ToList();

            return new PaymentSearchResultDto
            {
                Data = paymentDtos,
                TotalRecords = totalCount,
                PageNumber = searchDto.PageNumber,
                PageSize = searchDto.PageSize
            };
        }

        /// <summary>
        /// Get payment search summary statistics
        /// </summary>
        public async Task<PaymentSearchSummaryDto> GetPaymentSearchSummaryAsync(PaymentSearchDto searchDto)
        {
            var (totalAmount, totalFeeAmount, totalNetAmount, confirmedCount, paidCount, pendingCount) =
                await _paymentRepository.GetPaymentSummaryAsync(
                    searchDto.SearchTerm,
                    searchDto.PaymentStatus,
                    searchDto.BookingStatus,
                    searchDto.TransactionMethod,
                    searchDto.MinAmount,
                    searchDto.MaxAmount,
                    searchDto.FeePolicyId,
                    searchDto.PaymentDateFrom,
                    searchDto.PaymentDateTo,
                    searchDto.BookingDateFrom,
                    searchDto.BookingDateTo,
                    searchDto.BookingLocation
                );

            return new PaymentSearchSummaryDto
            {
                TotalPayments = confirmedCount + paidCount + pendingCount,
                TotalAmount = totalAmount,
                TotalFeeAmount = totalFeeAmount,
                TotalNetAmount = totalNetAmount,
                ConfirmedCount = confirmedCount,
                PaidCount = paidCount,
                PendingCount = pendingCount
            };
        }

        /// <summary>
        /// Get payment by ID with full details
        /// </summary>
        public async Task<PaymentDto?> GetPaymentByIdAsync(int paymentId)
        {
            var payment = await _paymentRepository.GetPaymentWithDetailsAsync(paymentId);
            return payment != null ? MapToPaymentDto(payment) : null;
        }

        /// <summary>
        /// Get all payments for a specific booking
        /// </summary>
        public async Task<PaymentDto?> GetPaymentsByBookingIdAsync(int bookingId)
        {
            var payments = await _paymentRepository.GetPaymentsByBookingIdAsync(bookingId);
            return payments != null ? MapToPaymentDto(payments) : null;
        }

        /// <summary>
        /// Get payments by customer ID
        /// </summary>
        public async Task<PaymentSearchResultDto> GetPaymentsByCustomerIdAsync(int customerId, int page = 1, int pageSize = 10)
        {
            var (payments, totalCount) = await _paymentRepository.GetPaymentsByCustomerIdAsync(customerId, page, pageSize);
            var paymentDtos = payments.Select(MapToPaymentDto).ToList();

            return new PaymentSearchResultDto
            {
                Data = paymentDtos,
                TotalRecords = totalCount,
                PageNumber = page,
                PageSize = pageSize
            };
        }

        /// <summary>
        /// Get payments by photographer ID
        /// </summary>
        public async Task<PaymentSearchResultDto> GetPaymentsByPhotographerIdAsync(int photographerId, int page = 1, int pageSize = 10)
        {
            var (payments, totalCount) = await _paymentRepository.GetPaymentsByPhotographerIdAsync(photographerId, page, pageSize);
            var paymentDtos = payments.Select(MapToPaymentDto).ToList();

            return new PaymentSearchResultDto
            {
                Data = paymentDtos,
                TotalRecords = totalCount,
                PageNumber = page,
                PageSize = pageSize
            };
        }

        #region Private Methods

        /// <summary>
        /// Map Payment entity to PaymentDto
        /// </summary>
        private static PaymentDto MapToPaymentDto(Payment payment)
        {
            return new PaymentDto
            {
                PaymentId = payment.PaymentId,
                BookingId = payment.BookingId,
                Amount = payment.Amount,
                FeePercent = payment.FeePercent,
                FeeAmount = payment.FeeAmount,
                NetAmount = payment.NetAmount,
                TransactionMethod = payment.TransactionMethod,
                TransactionReference = payment.TransactionReference,
                PaymentStatus = payment.PaymentStatus?.StatusName,
                PaymentDate = payment.PaymentDate,
                FeePolicy = payment.FeePolicy != null ? new FeePolicyDto
                {
                    FeePolicyId = payment.FeePolicy.FeePolicyId,
                    PolicyName = payment.FeePolicy.TransactionType, // Use TransactionType as PolicyName
                    FeePercent = payment.FeePolicy.FeePercent,
                    IsActive = payment.FeePolicy.IsActive,
                    EffectiveDate = payment.FeePolicy.EffectiveDate,
                    ExpiryDate = payment.FeePolicy.ExpiryDate
                } : null,
                Booking = payment.Booking != null ? new PaymentBookingDto
                {
                    BookingId = payment.Booking.BookingId,
                    BookingDate = payment.Booking.ScheduleAt, // Use ScheduleAt as BookingDate
                    EndTime = null, // Booking model doesn't have EndTime, set to null
                    Location = payment.Booking.LocationAddress, // Use LocationAddress as Location
                    Price = payment.Booking.Price,
                    Status = payment.Booking.Status?.StatusName,
                    Customer = payment.Booking.Customer != null ? new UserBasicDto
                    {
                        UserId = payment.Booking.Customer.UserId,
                        Name = payment.Booking.Customer.Name,
                        Email = payment.Booking.Customer.Email,
                        Phone = payment.Booking.Customer.Phone,
                        AvatarUrl = payment.Booking.Customer.AvatarUrl
                    } : null,
                    Photographer = payment.Booking.Photographer != null ? new UserBasicDto
                    {
                        UserId = payment.Booking.Photographer.UserId,
                        Name = payment.Booking.Photographer.Name,
                        Email = payment.Booking.Photographer.Email,
                        Phone = payment.Booking.Photographer.Phone,
                        AvatarUrl = payment.Booking.Photographer.AvatarUrl
                    } : null
                } : null
            };
        }

        #endregion
    }
}