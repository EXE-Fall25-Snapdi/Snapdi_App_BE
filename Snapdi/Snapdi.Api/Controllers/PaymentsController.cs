using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        [HttpPost("manual-payment")]
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
                    PaymentImageUrl = imageUrl
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

                return CreatedAtAction(nameof(GetPayment), new { id = payment.PaymentId }, new { success = true, id = payment.PaymentId, status = frontendStatus });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ManualPayment");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetPayment(int id)
        {
            var p = await _db.Payments.FindAsync(id);
            if (p == null) return NotFound(new { success = false, message = "Payment not found" });

            return Ok(new
            {
                p.PaymentId,
                p.BookingId,
                p.Amount,
                p.FeePolicyId,
                p.TransactionMethod,
                p.TransactionReference,
                p.PaymentStatusId,
                p.PaymentDate,
                p.PaymentImageUrl
            });
        }
    }
}
