using System.ComponentModel.DataAnnotations;

namespace Snapdi.Services.DTOs
{
    /// <summary>
    /// Model for PayOS payment information
    /// </summary>
    public class PaymentInformationModel
    {
        /// <summary>
        /// Booking ID associated with the payment
        /// </summary>
        [Required]
        public int BookingId { get; set; }

        /// <summary>
        /// Payment amount in VND
        /// </summary>
        [Required]
        [Range(1000, double.MaxValue, ErrorMessage = "Amount must be at least 1000 VND")]
        public double Amount { get; set; }

        /// <summary>
        /// Customer name
        /// </summary>
        [Required]
        public string CustomerName { get; set; } = string.Empty;

        /// <summary>
        /// Payment description
        /// </summary>
        public string? Description { get; set; }
    }

    /// <summary>
    /// PayOS Payment Response Model
    /// </summary>
    public class PaymentResponseModel
    {
        /// <summary>
        /// PayOS Order Code
        /// </summary>
        public long OrderCode { get; set; }

        /// <summary>
        /// Payment status
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Payment amount
        /// </summary>
        public double Amount { get; set; }

        /// <summary>
        /// Payment description
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Transaction reference from PayOS
        /// </summary>
        public string? TransactionReference { get; set; }

        /// <summary>
        /// Payment date
        /// </summary>
        public DateTime PaymentDate { get; set; }

        /// <summary>
        /// Associated booking ID
        /// </summary>
        public int BookingId { get; set; }

        /// <summary>
        /// Customer ID
        /// </summary>
        public int CustomerId { get; set; }

        /// <summary>
        /// Photographer ID
        /// </summary>
        public int PhotographerId { get; set; }

        /// <summary>
        /// Success status
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Error message if any
        /// </summary>
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// PayOS Create Payment Request
    /// </summary>
    public class CreatePaymentRequest
    {
        /// <summary>
        /// Booking ID for the payment
        /// </summary>
        [Required]
        public int BookingId { get; set; }
    }

    /// <summary>
    /// PayOS Payment Callback Data
    /// </summary>
    public class PayOSCallbackData
    {
        /// <summary>
        /// Order code from PayOS
        /// </summary>
        public long OrderCode { get; set; }

        /// <summary>
        /// Payment amount
        /// </summary>
        public double Amount { get; set; }

        /// <summary>
        /// Payment description
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Account number used for payment
        /// </summary>
        public string? AccountNumber { get; set; }

        /// <summary>
        /// Reference number from bank
        /// </summary>
        public string? Reference { get; set; }

        /// <summary>
        /// Transaction date time
        /// </summary>
        public DateTime TransactionDateTime { get; set; }

        /// <summary>
        /// Payment status: PAID, CANCELLED, PENDING
        /// </summary>
        public string Status { get; set; } = string.Empty;
    }
}
