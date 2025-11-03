using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Net.payOS;
using Net.payOS.Types;
using Snapdi.Repositories.Interfaces;
using Snapdi.Services.DTOs;
using Snapdi.Services.Interfaces;
using Snapdi.Services.Models;
using System.Net;
using System.Web;

namespace Snapdi.Services.Services
{
    /// <summary>
    /// PayOS payment service implementation
    /// Note: Install Net.payOS package and uncomment PayOS SDK calls when ready
    /// Command: dotnet add package Net.payOS --version 1.5.1
    /// </summary>
    public class PayOSService : IPayOSService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<PayOSService> _logger;
        private readonly IBookingRepository _bookingRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly PayOSSettings _payOSSettings;
        // Uncomment when PayOS package is installed:
        private readonly PayOS _payOS;
        private readonly HttpClient _httpClient;

        public PayOSService(
            IOptions<PayOSSettings> settings,
            ILogger<PayOSService> logger,
            IBookingRepository bookingRepository,
            IPaymentRepository paymentRepository,
            HttpClient httpClient)
        {
            _logger = logger;
            _bookingRepository = bookingRepository;
            _paymentRepository = paymentRepository;

            _payOSSettings = settings.Value;
            ConfigureHttpClient();
            _payOS = new PayOS(_payOSSettings.payOSClientId, _payOSSettings.payOSApiKey, _payOSSettings.payOSChecksumKey);
            _httpClient = httpClient;
        }

        private void ConfigureHttpClient()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;
        }

        /// <summary>
        /// Create payment URL for PayOS
        /// </summary>
        public async Task<string> CreatePaymentUrl(PaymentInformationModel model, HttpContext context)
        {
            try
            {


                // Configure security protocols
                //ServicePointManager.Expect100Continue = true;
                var domain = $"{context.Request.Scheme}://{context.Request.Host}";
                //ServicePointManager.DefaultConnectionLimit = 100;

                // Generate unique order code
                var orderCode = GenerateOrderCode(model.BookingId);

                // Get return and cancel URLs
                //var baseUrl = GetBaseUrl(context);
                var returnUrl = _payOSSettings.payOSReturnUrl;
                var cancelUrl = _payOSSettings.payOSCancelUrl;

                // Create PayOS payment data
                var paymentData = new PaymentData(
                    orderCode,
                    amount: (int)model.Amount,
                    description: model.Description ?? $"Payment for Photography Booking #{model.BookingId}",
                    items: new List<ItemData>
                    {
                        new ItemData(
                            name: "Booking Plan",
                            quantity: 1,
                            price: (int)(model.Amount)
                        )
                    },
                    // ✅ Return URL trỏ đến BE callback endpoint (KHÔNG phải FE)
                    returnUrl: $"{domain}/api/Payments/Checkout/PaymentCallbackPayOS",

                    // Cancel URL có thể trỏ đến FE
                    cancelUrl: $"{domain}/api/Payments/Checkout/PaymentCallbackPayOS" // Deep link hoặc FE URL
                );

                var createPayment = await _payOS.createPaymentLink(paymentData);

                if (createPayment != null)
                {
                    _logger.LogInformation($"PayOS payment created: OrderCode={orderCode}, BookingId={model.BookingId}, CheckoutUrl={createPayment.checkoutUrl}");
                    return createPayment.checkoutUrl;
                }

                throw new Exception("Failed to create PayOS payment link");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating PayOS payment URL for booking {BookingId}", model.BookingId);
                throw new Exception($"Error creating PayOS payment URL: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Process payment callback from PayOS
        /// </summary>
        public async Task<PaymentResponseModel> ProcessPaymentCallback(PayOSCallbackData callbackData)
        {
            try
            {


                // Verify payment with PayOS
                var paymentInfo = await _payOS.getPaymentLinkInformation(callbackData.OrderCode);

                if (paymentInfo != null)
                {
                    var isSuccess = paymentInfo.status == "PAID";

                    // Extract booking ID from order code
                    var bookingId = ExtractBookingIdFromOrderCode(callbackData.OrderCode);

                    return new PaymentResponseModel
                    {
                        OrderCode = callbackData.OrderCode,
                        Status = paymentInfo.status,
                        Amount = paymentInfo.amount,
                        TransactionReference = paymentInfo.transactions?.FirstOrDefault()?.reference ?? callbackData.Reference,
                        PaymentDate = callbackData.TransactionDateTime,
                        BookingId = bookingId,
                        Success = isSuccess,
                        ErrorMessage = isSuccess ? null : "Payment was not successful"
                    };
                }
                return null;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing PayOS callback for order {callbackData.OrderCode}");
                return new PaymentResponseModel
                {
                    OrderCode = callbackData.OrderCode,
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        /// <summary>
        /// Get payment information by order code
        /// </summary>
        public async Task<PaymentResponseModel> GetPaymentInfo(long orderCode)
        {
            try
            {
                var paymentInfo = await _payOS.getPaymentLinkInformation(orderCode);

                if (paymentInfo != null)
                {
                    var bookingId = ExtractBookingIdFromOrderCode(orderCode);

                    return new PaymentResponseModel
                    {
                        OrderCode = orderCode,
                        Status = paymentInfo.status,
                        Amount = paymentInfo.amount,
                        PaymentDate = DateTime.UtcNow,
                        BookingId = bookingId,
                        TransactionReference = paymentInfo.transactions?.FirstOrDefault()?.reference,
                        Success = paymentInfo.status == "PAID"
                    };
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting payment info for order {orderCode}");
                return new PaymentResponseModel
                {
                    OrderCode = orderCode,
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        /// <summary>
        /// Execute payment from callback URL (similar to PaymentExecute)
        /// </summary>
        public async Task<PaymentResponseModel> PaymentExecute(string linkResponse)
        {
            try
            {
                // Parse query parameters from the response URL
                var uri = new Uri(linkResponse);
                var query = HttpUtility.ParseQueryString(uri.Query);

                var orderCodeStr = query["orderCode"];
                var status = query["status"];
                var cancel = query["cancel"];

                if (string.IsNullOrEmpty(orderCodeStr))
                {
                    return new PaymentResponseModel
                    {
                        Success = false,
                        ErrorMessage = "Order code not found in response"
                    };
                }

                var orderCode = long.Parse(orderCodeStr);
                var bookingId = ExtractBookingIdFromOrderCode(orderCode);



                // Get payment information from PayOS
                var paymentInfo = await _payOS.getPaymentLinkInformation(orderCode);

                if (paymentInfo != null)
                {
                    bool isSuccess = paymentInfo.status == "PAID";

                    return new PaymentResponseModel
                    {
                        Success = isSuccess,
                        OrderCode = orderCode,
                        BookingId = bookingId,
                        Status = paymentInfo.status,
                        Amount = paymentInfo.amount,
                        TransactionReference = paymentInfo.transactions?.FirstOrDefault()?.reference ?? orderCodeStr,
                        PaymentDate = DateTime.UtcNow,
                        ErrorMessage = isSuccess ? null : "Payment was not successful"
                    };
                }

                return new PaymentResponseModel
                {
                    Success = false,
                    ErrorMessage = "Payment information not found"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing payment from response: {LinkResponse}", linkResponse);
                return new PaymentResponseModel
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        #region Private Methods

        /// <summary>
        /// Generate unique order code for PayOS
        /// </summary>
        private long GenerateOrderCode(int bookingId)
        {
            // Format: bookingId + timestamp (ensures uniqueness)
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var orderCodeStr = $"{bookingId}{timestamp}";

            // Ensure it fits in long range
            if (orderCodeStr.Length > 18)
            {
                orderCodeStr = orderCodeStr.Substring(0, 18);
            }

            return long.Parse(orderCodeStr);
        }

        /// <summary>
        /// Extract booking ID from order code
        /// </summary>
        private int ExtractBookingIdFromOrderCode(long orderCode)
        {
            try
            {
                var orderCodeStr = orderCode.ToString();
                // Booking ID is at the beginning, timestamp is the last 10 digits
                var timestampLength = 10;
                if (orderCodeStr.Length > timestampLength)
                {
                    var bookingIdStr = orderCodeStr.Substring(0, orderCodeStr.Length - timestampLength);
                    return int.Parse(bookingIdStr);
                }
                return 0;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Get base URL from HTTP context
        /// </summary>
        private string GetBaseUrl(HttpContext? context)
        {
            if (context == null)
            {
                return _configuration["App:BaseUrl"] ?? "https://localhost:7000";
            }

            var request = context.Request;
            return $"{request.Scheme}://{request.Host}";
        }

        #endregion
    }
}
