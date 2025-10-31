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
        private readonly IPaymentsService _paymentService;
        private readonly IPayOSService _payOSService;
        private readonly IBookingService _bookingService;
        private readonly IUserService _userService;


        public PaymentsController(
            ICloudinaryService cloudinaryService,
            SnapdiDbV2Context db,
            ILogger<PaymentsController> logger,
            IPaymentsService paymentService,
            IPayOSService payOSService,
            IBookingService bookingService,
            IUserService userService)
        {
            _cloudinaryService = cloudinaryService;
            _db = db;
            _logger = logger;
            _paymentService = paymentService;
            _payOSService = payOSService;
            _bookingService = bookingService;
            _userService = userService;
        }

        #region Manual Payment Methods

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

                // Find or create PaymentStatus 'Pending'
                var confirmedStatus = await _db.PaymentStatuses
                    .FirstOrDefaultAsync(ps => ps.StatusName.ToLower() == "pending");

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
                    TransactionMethod = "PayOS"
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
                    message = "Payment created successfully"
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
                    .FirstOrDefaultAsync(ps => ps.StatusName.ToLower() == "paid");

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
        public async Task<IActionResult> CancelManualPayment([FromBody] ManualPaymentRequestDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, message = "Invalid request data" });
                //var payment = await _db.Payments.FindAsync(paymentId);
                //if (payment == null)
                //    return NotFound(new { success = false, message = $"Payment {paymentId} not found" });
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
                //payment.PaymentStatusId = paidStatus.PaymentStatusId;
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
                _logger.LogError($"Error cancelling manual payment: {ex.Message}");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

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

        #endregion

        #region PayOS Integration Methods

        /// <summary>
        /// Create PayOS payment for a booking
        /// </summary>
        [HttpPost("payos/create-payment")]
        [Authorize]
        public async Task<ActionResult<object>> CreatePayOSPayment([FromBody] CreatePaymentRequest request)
        {
            try
            {
                await using var transaction = await _db.Database.BeginTransactionAsync();
                if (!ModelState.IsValid)
                {
                    await transaction.RollbackAsync();
                    return BadRequest(new
                    {
                        error = "Validation failed",
                        message = "Please check your input data",
                        details = ModelState.Where(x => x.Value.Errors.Count > 0)
                            .ToDictionary(
                                kvp => kvp.Key,
                                kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                            )
                    });
                }

                // Get current user
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    await transaction.RollbackAsync();
                    return BadRequest(new { error = "Invalid token", message = "User ID not found in token claims" });
                }

                // Verify booking exists and user has access
                var booking = await _bookingService.GetBookingByIdAsync(request.BookingId);
                if (booking == null)
                {
                    await transaction.RollbackAsync();
                    return NotFound(new { error = "Booking not found", message = $"Booking with ID {request.BookingId} does not exist" });
                }

                // Only customer can create payment for their booking
                if (booking.Customer?.UserId != userId)
                {
                    await transaction.RollbackAsync();
                    throw new Exception("You can only create payment for your own bookings");
                }

                var customer = await _userService.GetUserByIdAsync(userId);
                if (customer == null)
                {
                    await transaction.RollbackAsync();
                    return NotFound(new { error = "User not found", message = $"User with ID {userId} does not exist" });
                }

                // Set return and cancel URLs
                var paymentInformation = new PaymentInformationModel
                {
                    BookingId = request.BookingId,
                    Amount = (int)booking.Price * 0.2,
                    CustomerName = customer.Name,
                    Description = $"Payment for booking #{request.BookingId}"
                };

                // Create payment URL
                var paymentUrl = await _payOSService.CreatePaymentUrl(
                    paymentInformation,
                    HttpContext
                );

                await transaction.CommitAsync();

                return Ok(new
                {
                    success = true,
                    paymentUrl,
                    bookingId = request.BookingId,
                    message = "Payment URL created successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating PayOS payment for booking {request.BookingId}");
                return StatusCode(500, new
                {
                    error = "Internal server error",
                    message = "An error occurred while creating payment",
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Handle PayOS payment success callback
        /// </summary>
        [HttpGet("payos/payment-success")]
        public async Task<ActionResult> PayOSPaymentSuccess([FromQuery] long orderCode, [FromQuery] string? status)
        {
            try
            {
                _logger.LogInformation($"PayOS payment success callback: OrderCode={orderCode}, Status={status}");

                // Get payment info from PayOS
                var paymentResult = await _payOSService.GetPaymentInfo(orderCode);

                if (paymentResult.Success)
                {
                    // Here you can redirect to your frontend success page
                    // or return a success view
                    return Ok(new
                    {
                        success = true,
                        orderCode,
                        status = paymentResult.Status,
                        amount = paymentResult.Amount,
                        message = "Payment completed successfully"
                    });
                }

                return BadRequest(new
                {
                    success = false,
                    orderCode,
                    message = paymentResult.ErrorMessage ?? "Payment verification failed"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error handling PayOS payment success for order {orderCode}");
                return StatusCode(500, new
                {
                    error = "Internal server error",
                    message = "An error occurred while processing payment success"
                });
            }
        }

        /// <summary>
        /// Get payment information by order code (Admin only)
        /// </summary>
        [HttpGet("payos/payment-info/{orderCode}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<PaymentResponseModel>> GetPayOSPaymentInfo(long orderCode)
        {
            try
            {
                var result = await _payOSService.GetPaymentInfo(orderCode);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting PayOS payment info for order {orderCode}");
                return StatusCode(500, new
                {
                    error = "Internal server error",
                    message = "An error occurred while retrieving payment information"
                });
            }
        }

        #endregion

        #region Payment Search and Management

        /// <summary>
        /// Search payments with filtering and paging (Admin only)
        /// </summary>
        [HttpPost("search")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<PaymentSearchResultDto>> SearchPayments(PaymentSearchDto searchDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        error = "Validation failed",
                        message = "Please check your input data",
                        details = ModelState.Where(x => x.Value.Errors.Count > 0)
                            .ToDictionary(
                                kvp => kvp.Key,
                                kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                            )
                    });
                }

                var result = await _paymentService.SearchPaymentsAsync(searchDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "Internal server error",
                    message = "An error occurred while searching payments",
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Get payment search summary statistics (Admin only)
        /// </summary>
        [HttpPost("search/summary")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<PaymentSearchSummaryDto>> GetPaymentSearchSummary(PaymentSearchDto searchDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        error = "Validation failed",
                        message = "Please check your input data",
                        details = ModelState.Where(x => x.Value.Errors.Count > 0)
                            .ToDictionary(
                                kvp => kvp.Key,
                                kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                            )
                    });
                }

                var summary = await _paymentService.GetPaymentSearchSummaryAsync(searchDto);
                return Ok(summary);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "Internal server error",
                    message = "An error occurred while getting payment summary",
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Get payments for current user (Customer or Photographer)
        /// </summary>
        [HttpGet("my-payments")]
        [Authorize]
        public async Task<ActionResult<PaymentSearchResultDto>> GetMyPayments([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return BadRequest(new { error = "Invalid token", message = "User ID not found in token claims" });
                }

                PaymentSearchResultDto result;

                // If user is photographer, get payments where they are the photographer
                if (userRole == "PHOTOGRAPHER")
                {
                    result = await _paymentService.GetPaymentsByPhotographerIdAsync(userId, page, pageSize);
                }
                // Otherwise, get payments where they are the customer
                else
                {
                    result = await _paymentService.GetPaymentsByCustomerIdAsync(userId, page, pageSize);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "Internal server error",
                    message = "An error occurred while retrieving your payments",
                    details = ex.Message
                });
            }
        }

        /// <summary>
        /// Get detailed payment information by ID (Admin, or users involved in the payment)
        /// </summary>
        [HttpGet("details/{id}")]
        [Authorize]
        public async Task<ActionResult<PaymentDto>> GetPaymentDetails(int id)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return BadRequest(new { error = "Invalid token", message = "User ID not found in token claims" });
                }

                var payment = await _paymentService.GetPaymentByIdAsync(id);
                if (payment == null)
                {
                    return NotFound(new { error = "Payment not found", message = $"Payment with ID {id} does not exist" });
                }

                // Verify user has access to this payment (unless admin)
                if (userRole != "ADMIN")
                {
                    var hasAccess = payment.Booking?.Customer?.UserId == userId ||
                                   payment.Booking?.Photographer?.UserId == userId;

                    if (!hasAccess)
                    {
                        throw new Exception("You can only access your own payment details");
                    }
                }

                return Ok(payment);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "Internal server error",
                    message = "An error occurred while retrieving payment details",
                    details = ex.Message
                });
            }
        }

        [HttpGet("Checkout/PaymentCallbackPayOS")]
        public async Task<IActionResult> PaymentCallbackPayOS()
        {
            try
            {
                _logger.LogInformation("PayOS return callback received");
                
                var query = HttpContext.Request.Query;
                var code = query["code"].ToString();
                var id = query["id"].ToString();
                var cancel = query["cancel"].ToString();
                var status = query["status"].ToString();
                var orderCode = query["orderCode"].ToString();
                
                _logger.LogInformation($"PayOS params: code={code}, id={id}, cancel={cancel}, status={status}, orderCode={orderCode}");

                // ✅ Xác định trạng thái thanh toán: CHỈ code=00 và cancel!=true mới là paid
                string paymentStatus;
                if (code == "00" && cancel != "true")
                {
                    paymentStatus = "paid";
                    
                    // Verify và cập nhật payment status
                    if (!string.IsNullOrEmpty(orderCode) && long.TryParse(orderCode, out long orderCodeLong))
                    {
                        try
                        {
                            var paymentInfo = await _payOSService.GetPaymentInfo(orderCodeLong);
                            if (paymentInfo != null)
                            {
                                await _bookingService.ProcessPaymentCallbackAsync(paymentInfo);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"Error processing payment callback: {ex.Message}");
                        }
                    }
                }
                else
                {
                    // ✅ TẤT CẢ các trường hợp khác đều là cancelled
                    paymentStatus = "cancelled";
                }

                _logger.LogInformation($"Final payment status: {paymentStatus}");

                // Return HTML page với deep link
                var html = $@"
<!DOCTYPE html>
<html>
<head>
    <title>Kết quả thanh toán</title>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <style>
        * {{ margin: 0; padding: 0; box-sizing: border-box; }}
        body {{ 
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Arial, sans-serif;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            min-height: 100vh;
            display: flex;
            align-items: center;
            justify-content: center;
            padding: 20px;
        }}
        .container {{
            background: white;
            border-radius: 20px;
            padding: 40px;
            max-width: 400px;
            width: 100%;
            text-align: center;
            box-shadow: 0 20px 60px rgba(0,0,0,0.3);
        }}
        .spinner {{
            border: 3px solid #f3f3f3;
            border-top: 3px solid #667eea;
            border-radius: 50%;
            width: 40px;
            height: 40px;
            animation: spin 1s linear infinite;
            margin: 20px auto;
        }}
        @keyframes spin {{
            0% {{ transform: rotate(0deg); }}
            100% {{ transform: rotate(360deg); }}
        }}
        h1 {{ font-size: 24px; color: #333; }}
        p {{ color: #666; margin-top: 12px; }}
        .icon {{
            width: 80px;
            height: 80px;
            margin: 0 auto 24px;
            border-radius: 50%;
            display: flex;
            align-items: center;
            justify-content: center;
            font-size: 40px;
        }}
        .icon.success {{ background: #d4edda; color: #28a745; }}
        .icon.failed {{ background: #f8d7da; color: #dc3545; }}
        .code {{
            background: #f5f5f5;
            padding: 12px;
            border-radius: 8px;
            margin: 20px 0;
            font-family: monospace;
            color: #333;
        }}
        .btn {{
            display: inline-block;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            text-decoration: none;
            padding: 16px 32px;
            border-radius: 12px;
            font-weight: 600;
            font-size: 16px;
            transition: transform 0.2s;
            border: none;
            cursor: pointer;
            width: 100%;
            margin-top: 20px;
        }}
        .btn:hover {{ transform: translateY(-2px); }}
        .auto-redirect {{
            font-size: 13px;
            color: #999;
            margin-top: 16px;
        }}
    </style>
</head>
<body>
    <div class='container' id='content'>
        <div class='spinner'></div>
        <h1>Đang xử lý...</h1>
        <p>Vui lòng đợi trong giây lát</p>
    </div>
    
    <script>
        const paymentStatus = '{paymentStatus}'; // ✅ Chỉ 'paid' hoặc 'cancelled'
        const code = '{code}';
        const orderCode = '{orderCode}';
        
        // Deep link với status rõ ràng
        const deepLink = `snapdi://payment/result?status=${{paymentStatus}}&code=${{code}}&orderCode=${{orderCode}}`;
        
        console.log('Payment Status:', paymentStatus);
        console.log('Deep link:', deepLink);
        
        // Try to open deep link after 500ms
        setTimeout(function() {{
            window.location.href = deepLink;
        }}, 500);
        
        // Show fallback UI after 2 seconds
        setTimeout(function() {{
            const iconClass = paymentStatus === 'paid' ? 'success' : 'failed';
            const iconSymbol = paymentStatus === 'paid' ? '✓' : '✕';
            const title = paymentStatus === 'paid' ? 'Thanh toán thành công!' : 'Thanh toán không thành công';
            const description = paymentStatus === 'paid' 
                ? 'Booking của bạn đã được xác nhận. Vui lòng mở ứng dụng SnapDi để xem chi tiết.'
                : 'Giao dịch đã bị hủy hoặc thất bại. Vui lòng thử lại.';
            
            document.getElementById('content').innerHTML = `
                <div class='icon ${{iconClass}}'>${{iconSymbol}}</div>
                <h1>${{title}}</h1>
                <p>${{description}}</p>
                <div class='code'>Mã giao dịch: ${{code}}</div>
                <button class='btn' onclick='openApp()'>Mở ứng dụng SnapDi</button>
                <p class='auto-redirect'>Nếu không tự động chuyển hướng, vui lòng bấm nút bên trên</p>
            `;
        }}, 2000);
        
        function openApp() {{
            window.location.href = deepLink;
            setTimeout(function() {{
                alert('Không thể mở ứng dụng tự động. Vui lòng mở ứng dụng SnapDi thủ công.');
            }}, 1000);
        }}
    </script>
</body>
</html>";

                return Content(html, "text/html");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in PaymentCallbackPayOS: {ex.Message}");
                
                // ✅ Error cũng trả về cancelled
                var errorHtml = @"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <style>
        body { font-family: Arial, sans-serif; text-align: center; padding: 50px; background: #f5f5f5; }
        .container { background: white; padding: 40px; border-radius: 16px; max-width: 400px; margin: 0 auto; }
        .icon { font-size: 60px; margin-bottom: 20px; }
        h2 { color: #333; margin-bottom: 16px; }
        p { color: #666; margin-bottom: 24px; }
        .btn { display: inline-block; background: #dc3545; color: white; text-decoration: none; padding: 14px 28px; border-radius: 8px; font-weight: 600; border: none; cursor: pointer; }
    </style>
</head>
<body>
    <div class='container'>
        <div class='icon'>⚠️</div>
        <h2>Có lỗi xảy ra</h2>
        <p>Không thể xử lý kết quả thanh toán. Vui lòng liên hệ hỗ trợ.</p>
        <button class='btn' onclick=""window.location.href='snapdi://payment/result?status=cancelled&error=processing_failed'"">
            Quay lại ứng dụng
        </button>
    </div>
</body>
</html>";
        
        return Content(errorHtml, "text/html");
    }
}
        #endregion
    }
}
