using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Snapdi.Repositories.Context;
using Snapdi.Repositories.Models;
using Snapdi.Services.DTOs;
using Snapdi.Services.Interfaces;
using System.Security.Claims;

namespace Snapdi.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly ICloudinaryService _cloudinaryService;
        private readonly SnapdiDbV2Context _db;
        private readonly ILogger<PaymentsController> _logger;

        public PaymentsController(ICloudinaryService cloudinaryService, SnapdiDbV2Context db, ILogger<PaymentsController> logger)
        {
            _cloudinaryService = cloudinaryService;
            _db = db;
            _logger = logger;
        }

        /// <summary>
        /// Accept multipart form: BookingId, Amount, TransactionReference, proofImage (file).
        /// Uploads image to Cloudinary and creates a Payment record with FeePolicyId = 1 and status = 'done'.
        /// </summary>
        [HttpPost("manual-payment-old")]
        [RequestSizeLimit(30_000_000)]
        public async Task<IActionResult> ManualPayment()
        {
            try
            {
                var form = await Request.ReadFormAsync();

                if (!form.TryGetValue("BookingId", out var bookingVals) || !int.TryParse(bookingVals.FirstOrDefault(), out var bookingId))
                {
                    return BadRequest(new { success = false, message = "BookingId is required" });
                }

                if (!form.TryGetValue("Amount", out var amountVals) || !decimal.TryParse(amountVals.FirstOrDefault(), out var amount))
                {
                    return BadRequest(new { success = false, message = "Amount is required" });
                }

                var transactionReference = form.TryGetValue("TransactionReference", out var txVals) ? txVals.FirstOrDefault() ?? string.Empty : string.Empty;

                // find booking
                var booking = await _db.Bookings.FindAsync(bookingId);
                if (booking == null)
                {
                    return NotFound(new { success = false, message = $"Booking {bookingId} not found" });
                }

                // upload file if present
                string? imageUrl = null;
                var file = form.Files.FirstOrDefault(f => f.Name == "proofImage" || f.Name == "file" || f.Name == "image");
                if (file != null && file.Length > 0)
                {
                    if (!_cloudinaryService.IsValidImage(file))
                        return BadRequest(new { success = false, message = "Invalid image file" });

                    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                    {
                        return Unauthorized(new { success = false, message = "Invalid token" });
                    }

                    var uploadReq = new CloudinaryUploadRequestDto
                    {
                        UserId = userId,
                        UploadType = "payment",
                        Overwrite = false
                    };

                    var uploadResult = await _cloudinaryService.UploadImageAsync(file, uploadReq);
                    imageUrl = uploadResult.SecureUrl ?? uploadResult.Url;
                }

                // determine PaymentStatusId: prefer 'done', then 'paid' or 'completed'
                var status = _db.PaymentStatuses.FirstOrDefault(ps => ps.StatusName.ToLower() == "done"
                                                                         || ps.StatusName.ToLower() == "paid"
                                                                         || ps.StatusName.ToLower() == "completed"
                                                                         || ps.StatusName.ToLower() == "success");

                // If not found, create a 'Done' status so payments will have a 'Done' state
                if (status == null)
                {
                    status = new PaymentStatus { StatusName = "Done" };
                    _db.PaymentStatuses.Add(status);
                    await _db.SaveChangesAsync();
                }

                int? paymentStatusId = status.PaymentStatusId;

                var payment = new Payment
                {
                    BookingId = bookingId,
                    Amount = (double)amount,
                    FeePolicyId = 1,
                    TransactionMethod = "Manual",
                    TransactionReference = transactionReference,
                    PaymentStatusId = paymentStatusId,
                    PaymentDate = DateTime.UtcNow,
                    //    PaymentImageUrl = imageUrl
                };

                _db.Payments.Add(payment);
                await _db.SaveChangesAsync();

                _logger.LogInformation("Created manual payment {PaymentId} for booking {BookingId}", payment.PaymentId, bookingId);

                // Normalize status to frontend-friendly keywords
                var statusName = status?.StatusName?.ToLower() ?? "done";
                string frontendStatus = statusName switch
                {
                    var s when s.Contains("done") || s.Contains("paid") || s.Contains("success") || s.Contains("completed") => "approved",
                    var s when s.Contains("pending") => "pending",
                    var s when s.Contains("rejected") || s.Contains("failed") => "rejected",
                    _ => "pending",
                };

                return CreatedAtAction(nameof(GetPaymentById), new { id = payment.PaymentId }, new { success = true, id = payment.PaymentId, status = frontendStatus });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ManualPayment");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [Authorize]
        [HttpPost("confirm-manual-payment")]
        public async Task<IActionResult> ConfirmManualPayment([FromBody] ManualPaymentRequestDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, message = "Invalid request data" });

                // Validate booking exists
                var booking = await _db.Bookings.FindAsync(dto.BookingId);
                if (booking == null)
                    return NotFound(new { success = false, message = $"Booking {dto.BookingId} not found" });

                // Get userId from token
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                    return Unauthorized(new { success = false, message = "User not authenticated" });

                // Find or create PaymentStatus 'Confirmed'
                var confirmedStatus = await _db.PaymentStatuses
                    .FirstOrDefaultAsync(ps => ps.StatusName.ToLower() == "confirmed");

                if (confirmedStatus == null)
                {
                    return NotFound(new { success = false, message = $"Payment Status {confirmedStatus} not found" });
                }

                // Find or create BookingStatus 'Confirmed'
                var confirmedBookingStatus = await _db.BookingStatuses
                    .FirstOrDefaultAsync(ps => ps.StatusName.ToLower() == "confirmed");

                if (confirmedBookingStatus == null)
                {
                    return NotFound(new { success = false, message = $"Booking Status {confirmedBookingStatus} not found" });
                }

                var policy = await _db.FeePolicies.FindAsync(dto.FeePolicyId);
                if (policy == null || !policy.IsActive || policy.ExpiryDate < DateTime.UtcNow)
                {
                    return BadRequest(new { success = false, message = "Invalid or inactive Fee Policy" });
                }

                var AmountCustomerPay = Math.Round(booking.Price * 20 / 100);
                var feeAmount = Math.Round(AmountCustomerPay * policy.FeePercent / 100);
                var netAmount = AmountCustomerPay - feeAmount;

                // Create Payment record
                var payment = new Payment
                {
                    BookingId = dto.BookingId,
                    Amount = AmountCustomerPay,
                    FeeAmount = feeAmount,
                    NetAmount = netAmount,
                    FeePercent = policy.FeePercent,
                    PaymentStatusId = confirmedStatus.PaymentStatusId,
                    FeePolicyId = dto.FeePolicyId, // Fixed as per requirement
                    PaymentDate = DateTime.UtcNow,
                    TransactionMethod = "Manual"
                };

                _db.Payments.Add(payment);
                await _db.SaveChangesAsync();

                // Update Booking status to Pending
                booking.StatusId = (int)(confirmedBookingStatus.StatusId);
                await _db.SaveChangesAsync();

                _logger.LogInformation($"Payment created: {payment.PaymentId} for booking {dto.BookingId}");

                return CreatedAtAction(nameof(GetPaymentById), new { id = payment.PaymentId }, new
                {
                    success = true,
                    paymentId = payment.PaymentId,
                    status = "done",
                    message = "Payment confirmed successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error confirming manual payment: {ex.Message}");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        [Authorize]
        [HttpPut("confirm-paid")]
        public async Task<IActionResult> ConfirmPaid([FromBody] ManualPaymentRequestDto dto, int paymentId)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, message = "Invalid request data" });
                var payment = await _db.Payments.FindAsync(paymentId);
                if (payment == null)
                    return NotFound(new { success = false, message = $"Payment {paymentId} not found" });
                // Validate booking exists
                var booking = await _db.Bookings.FindAsync(dto.BookingId);
                if (booking == null)
                    return NotFound(new { success = false, message = $"Booking {dto.BookingId} not found" });

                // Get userId from token
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                    return Unauthorized(new { success = false, message = "User not authenticated" });

                // Find or create PaymentStatus 'Paid'
                var paidStatus = await _db.PaymentStatuses
                    .FirstOrDefaultAsync(ps => ps.StatusName.ToLower() == "paid");

                if (paidStatus == null)
                {
                    return NotFound(new { success = false, message = $"Payment Status {paidStatus} not found" });
                }

                // Find or create BookingStatus 'Confirmed'
                var completedBookingStatus = await _db.BookingStatuses
                    .FirstOrDefaultAsync(ps => ps.StatusName.ToLower() == "confirmed");

                if (completedBookingStatus == null)
                {
                    return NotFound(new { success = false, message = $"Booking Status {completedBookingStatus} not found" });
                }

                payment.PaymentStatusId = paidStatus.PaymentStatusId;

                // Update Booking status to Paid
                booking.StatusId = (int)(completedBookingStatus.StatusId);
                await _db.SaveChangesAsync();

                _logger.LogInformation($"Payment marked as paid for booking {dto.BookingId}");

                return Ok(new
                {
                    success = true,
                    message = "Payment marked as paid successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error confirming manual payment: {ex.Message}");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        [Authorize]
        [HttpPut("cancel-manual-payment")]
        public async Task<IActionResult> CancelManualPayment([FromBody] ManualPaymentRequestDto dto, int paymentId)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, message = "Invalid request data" });
                var payment = await _db.Payments.FindAsync(paymentId);
                if (payment == null)
                    return NotFound(new { success = false, message = $"Payment {paymentId} not found" });
                // Validate booking exists
                var booking = await _db.Bookings.FindAsync(dto.BookingId);
                if (booking == null)
                    return NotFound(new { success = false, message = $"Booking {dto.BookingId} not found" });

                // Get userId from token
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                    return Unauthorized(new { success = false, message = "User not authenticated" });

                // Find or create PaymentStatus 'Refunded'
                var paidStatus = await _db.PaymentStatuses
                    .FirstOrDefaultAsync(ps => ps.StatusName.ToLower() == "refunded");

                if (paidStatus == null)
                {
                    return NotFound(new { success = false, message = $"Payment Status {paidStatus} not found" });
                }

                // Find or create BookingStatus 'Cancelled'
                var canceledBookingStatus = await _db.BookingStatuses
                    .FirstOrDefaultAsync(ps => ps.StatusName.ToLower() == "cancelled");

                if (canceledBookingStatus == null)
                {
                    return NotFound(new { success = false, message = $"Booking Status {canceledBookingStatus} not found" });
                }

                //Update Payment status to Cancelled
                payment.PaymentStatusId = paidStatus.PaymentStatusId;
                // Update Booking status to Cancelled
                booking.StatusId = (int)(canceledBookingStatus.StatusId);
                await _db.SaveChangesAsync();

                _logger.LogInformation($"Payment cancelled for booking {dto.BookingId}");

                return Ok(new
                {
                    success = true,
                    message = "Payment cancelled successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error confirming manual payment: {ex.Message}");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        //[HttpGet("{id}")]
        //[Authorize]
        //public async Task<IActionResult> GetPayment(int id)
        //{
        //    var p = await _db.Payments.FindAsync(id);
        //    if (p == null) return NotFound(new { success = false, message = "Payment not found" });

        //    return Ok(new
        //    {
        //        p.PaymentId,
        //        p.BookingId,
        //        p.Amount,
        //        p.FeePolicyId,
        //        p.TransactionMethod,
        //        p.PaymentStatusId,
        //        p.PaymentDate
        //    });
        //}

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPaymentById(int id)
        {
            var payment = await _db.Payments
                .Include(p => p.PaymentStatus)
                .FirstOrDefaultAsync(p => p.PaymentId == id);

            if (payment == null)
                return NotFound(new { success = false, message = "Payment not found" });

            return Ok(new
            {
                success = true,
                id = payment.PaymentId,
                bookingId = payment.BookingId,
                amount = payment.Amount,
                status = payment.PaymentStatus?.StatusName ?? "Unknown",
                paymentDate = payment.PaymentDate
            });
        }
    }
}
