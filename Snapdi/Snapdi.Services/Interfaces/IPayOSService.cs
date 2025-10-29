using Microsoft.AspNetCore.Http;
using Snapdi.Services.DTOs;

namespace Snapdi.Services.Interfaces
{
    /// <summary>
    /// Interface for PayOS payment service
    /// </summary>
    public interface IPayOSService
    {
        /// <summary>
        /// Create payment URL for PayOS
        /// </summary>
        /// <param name="model">Payment information</param>
        /// <param name="context">HTTP context</param>
        /// <returns>Payment URL</returns>
        Task<string> CreatePaymentUrl(PaymentInformationModel model, HttpContext context);

        /// <summary>
        /// Process payment callback from PayOS
        /// </summary>
        /// <param name="callbackData">Callback data from PayOS</param>
        /// <returns>Payment response</returns>
        Task<PaymentResponseModel> ProcessPaymentCallback(PayOSCallbackData callbackData);

        /// <summary>
        /// Get payment information by order code
        /// </summary>
        /// <param name="orderCode">PayOS order code</param>
        /// <returns>Payment information</returns>
        Task<PaymentResponseModel> GetPaymentInfo(long orderCode);

        /// <summary>
        /// Execute payment from callback URL
        /// </summary>
        /// <param name="linkResponse">Payment callback URL with query parameters</param>
        /// <returns>Payment response</returns>
        Task<PaymentResponseModel> PaymentExecute(string linkResponse);
    }
}
