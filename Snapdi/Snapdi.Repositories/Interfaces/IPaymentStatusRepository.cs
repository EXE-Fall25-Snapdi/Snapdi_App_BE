using Snapdi.Repositories.Models;

namespace Snapdi.Repositories.Interfaces
{
    /// <summary>
    /// Repository interface for PaymentStatus operations
    /// </summary>
    public interface IPaymentStatusRepository : IBaseRepository<PaymentStatus>
    {
        /// <summary>
        /// Get payment status by name
        /// </summary>
        /// <param name="statusName">Status name (case insensitive)</param>
        /// <returns>PaymentStatus or null if not found</returns>
        Task<PaymentStatus?> GetByNameAsync(string statusName);

        /// <summary>
        /// Get all active payment statuses
        /// </summary>
        /// <returns>List of payment statuses</returns>
        Task<IEnumerable<PaymentStatus>> GetAllActiveAsync();

        /// <summary>
        /// Check if payment status exists by name
        /// </summary>
        /// <param name="statusName">Status name to check</param>
        /// <returns>True if exists, false otherwise</returns>
        Task<bool> ExistsByNameAsync(string statusName);
    }
}