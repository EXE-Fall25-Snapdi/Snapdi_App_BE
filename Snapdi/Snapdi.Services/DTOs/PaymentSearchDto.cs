using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snapdi.Services.DTOs
{
    /// <summary>
    /// DTO for payment search with filtering and paging
    /// </summary>
    public class PaymentSearchDto
    {
        /// <summary>
        /// Search term for customer name, photographer name, transaction reference, or booking location
        /// </summary>
        public string? SearchTerm { get; set; }

        /// <summary>
        /// Filter by payment status (e.g., "Pending", "Confirmed", "Paid", "Refunded")
        /// </summary>
        public string? PaymentStatus { get; set; }

        /// <summary>
        /// Filter by booking status
        /// </summary>
        public string? BookingStatus { get; set; }

        /// <summary>
        /// Filter by transaction method (e.g., "Manual", "Online", "Bank Transfer")
        /// </summary>
        public string? TransactionMethod { get; set; }

        /// <summary>
        /// Minimum payment amount
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "Minimum amount must be a positive number")]
        public double? MinAmount { get; set; }

        /// <summary>
        /// Maximum payment amount
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "Maximum amount must be a positive number")]
        public double? MaxAmount { get; set; }

        /// <summary>
        /// Filter by fee policy ID
        /// </summary>
        public int? FeePolicyId { get; set; }

        /// <summary>
        /// Payment date range filter - from date
        /// </summary>
        public DateTime? PaymentDateFrom { get; set; }

        /// <summary>
        /// Payment date range filter - to date  
        /// </summary>
        public DateTime? PaymentDateTo { get; set; }

        /// <summary>
        /// Booking date range filter - from date
        /// </summary>
        public DateTime? BookingDateFrom { get; set; }

        /// <summary>
        /// Booking date range filter - to date
        /// </summary>
        public DateTime? BookingDateTo { get; set; }

        /// <summary>
        /// Filter by booking city/location
        /// </summary>
        public string? BookingLocation { get; set; }

        /// <summary>
        /// Sort by field: "paymentDate", "amount", "customerName", "photographerName", "bookingDate", "status"
        /// </summary>
        [RegularExpression(@"^(paymentDate|amount|customerName|photographerName|bookingDate|status)$", 
            ErrorMessage = "Sort by must be 'paymentDate', 'amount', 'customerName', 'photographerName', 'bookingDate', or 'status'")]
        public string? SortBy { get; set; } = "paymentDate";

        /// <summary>
        /// Sort direction: "asc" or "desc"
        /// </summary>
        [RegularExpression(@"^(asc|desc)$", ErrorMessage = "Sort direction must be 'asc' or 'desc'")]
        public string? SortDirection { get; set; } = "desc";

        /// <summary>
        /// Page number (starts from 1)
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "Page number must be greater than 0")]
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// Number of items per page (max 100)
        /// </summary>
        [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100")]
        public int PageSize { get; set; } = 10;
    }

    /// <summary>
    /// Response DTO for payment search results
    /// </summary>
    public class PaymentSearchResultDto
    {
        /// <summary>
        /// List of payments matching the search criteria
        /// </summary>
        public List<PaymentDto> Data { get; set; } = new();

        /// <summary>
        /// Total number of records found
        /// </summary>
        public int TotalRecords { get; set; }

        /// <summary>
        /// Current page number
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// Number of items per page
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Total number of pages
        /// </summary>
        public int TotalPages => (int)Math.Ceiling((double)TotalRecords / PageSize);

        /// <summary>
        /// Whether there is a next page
        /// </summary>
        public bool HasNextPage => PageNumber < TotalPages;

        /// <summary>
        /// Whether there is a previous page
        /// </summary>
        public bool HasPreviousPage => PageNumber > 1;
    }

    /// <summary>
    /// DTO for payment information in search results
    /// </summary>
    public class PaymentDto
    {
        /// <summary>
        /// Payment ID
        /// </summary>
        public int PaymentId { get; set; }

        /// <summary>
        /// Associated booking ID
        /// </summary>
        public int? BookingId { get; set; }

        /// <summary>
        /// Payment amount
        /// </summary>
        public double Amount { get; set; }

        /// <summary>
        /// Fee percentage applied
        /// </summary>
        public double? FeePercent { get; set; }

        /// <summary>
        /// Fee amount deducted
        /// </summary>
        public double? FeeAmount { get; set; }

        /// <summary>
        /// Net amount after fee deduction
        /// </summary>
        public double? NetAmount { get; set; }

        /// <summary>
        /// Transaction method used
        /// </summary>
        public string? TransactionMethod { get; set; }

        /// <summary>
        /// Transaction reference number
        /// </summary>
        public string? TransactionReference { get; set; }

        /// <summary>
        /// Payment status
        /// </summary>
        public string? PaymentStatus { get; set; }

        /// <summary>
        /// Payment date and time
        /// </summary>
        public DateTime PaymentDate { get; set; }

        /// <summary>
        /// Fee policy information
        /// </summary>
        public FeePolicyDto? FeePolicy { get; set; }

        /// <summary>
        /// Booking information
        /// </summary>
        public PaymentBookingDto? Booking { get; set; }
    }

    /// <summary>
    /// DTO for fee policy information
    /// </summary>
    public class FeePolicyDto
    {
        /// <summary>
        /// Fee policy ID
        /// </summary>
        public int FeePolicyId { get; set; }

        /// <summary>
        /// Transaction type (used as policy name)
        /// </summary>
        public string? PolicyName { get; set; }

        /// <summary>
        /// Fee percentage
        /// </summary>
        public double FeePercent { get; set; }

        /// <summary>
        /// Whether the policy is active
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Effective date of the policy
        /// </summary>
        public DateTime EffectiveDate { get; set; }

        /// <summary>
        /// Expiry date of the policy
        /// </summary>
        public DateTime? ExpiryDate { get; set; }
    }

    /// <summary>
    /// DTO for booking information in payment search (renamed to avoid conflict)
    /// </summary>
    public class PaymentBookingDto
    {
        /// <summary>
        /// Booking ID
        /// </summary>
        public int BookingId { get; set; }

        /// <summary>
        /// Booking date and time
        /// </summary>
        public DateTime? BookingDate { get; set; }

        /// <summary>
        /// Booking end time
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// Booking location
        /// </summary>
        public string? Location { get; set; }

        /// <summary>
        /// Booking price
        /// </summary>
        public double Price { get; set; }

        /// <summary>
        /// Booking status
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// Customer information
        /// </summary>
        public UserBasicDto? Customer { get; set; }

        /// <summary>
        /// Photographer information
        /// </summary>
        public UserBasicDto? Photographer { get; set; }
    }

    /// <summary>
    /// Basic user information for payment search results
    /// </summary>
    public class UserBasicDto
    {
        /// <summary>
        /// User ID
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// User name
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// User email
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// User phone number
        /// </summary>
        public string? Phone { get; set; }

        /// <summary>
        /// User avatar URL
        /// </summary>
        public string? AvatarUrl { get; set; }
    }

    /// <summary>
    /// Summary information for payment search results
    /// </summary>
    public class PaymentSearchSummaryDto
    {
        /// <summary>
        /// Total payments found
        /// </summary>
        public int TotalPayments { get; set; }

        /// <summary>
        /// Total amount of all payments
        /// </summary>
        public double TotalAmount { get; set; }

        /// <summary>
        /// Total fee amount collected
        /// </summary>
        public double TotalFeeAmount { get; set; }

        /// <summary>
        /// Total net amount paid to photographers
        /// </summary>
        public double TotalNetAmount { get; set; }

        /// <summary>
        /// Number of confirmed payments
        /// </summary>
        public int ConfirmedCount { get; set; }

        /// <summary>
        /// Number of paid payments
        /// </summary>
        public int PaidCount { get; set; }

        /// <summary>
        /// Number of pending payments
        /// </summary>
        public int PendingCount { get; set; }

        /// <summary>
        /// Average payment amount
        /// </summary>
        public double AverageAmount => TotalPayments > 0 ? TotalAmount / TotalPayments : 0;

        /// <summary>
        /// Summary message
        /// </summary>
        public string Message => $"Found {TotalPayments} payment(s): " +
                                $"{ConfirmedCount} confirmed, {PaidCount} paid, {PendingCount} pending. " +
                                $"Total amount: {TotalAmount:C}, Average: {AverageAmount:C}";
    }
}
