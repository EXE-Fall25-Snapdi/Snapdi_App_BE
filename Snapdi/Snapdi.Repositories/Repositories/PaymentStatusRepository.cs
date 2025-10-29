using Microsoft.EntityFrameworkCore;
using Snapdi.Repositories.Context;
using Snapdi.Repositories.Interfaces;
using Snapdi.Repositories.Models;

namespace Snapdi.Repositories.Repositories
{
    /// <summary>
    /// Repository implementation for PaymentStatus operations
    /// </summary>
    public class PaymentStatusRepository : BaseRepository<PaymentStatus>, IPaymentStatusRepository
    {
        public PaymentStatusRepository(SnapdiDbV2Context context) : base(context)
        {
        }

        /// <summary>
        /// Get payment status by name (case insensitive)
        /// </summary>
        public async Task<PaymentStatus?> GetByNameAsync(string statusName)
        {
            if (string.IsNullOrWhiteSpace(statusName))
                return null;

            return await _context.PaymentStatuses
                .FirstOrDefaultAsync(ps => ps.StatusName.ToLower() == statusName.ToLower());
        }

        /// <summary>
        /// Get all active payment statuses
        /// </summary>
        public async Task<IEnumerable<PaymentStatus>> GetAllActiveAsync()
        {
            return await _context.PaymentStatuses
                .OrderBy(ps => ps.PaymentStatusId)
                .ToListAsync();
        }

        /// <summary>
        /// Check if payment status exists by name
        /// </summary>
        public async Task<bool> ExistsByNameAsync(string statusName)
        {
            if (string.IsNullOrWhiteSpace(statusName))
                return false;

            return await _context.PaymentStatuses
                .AnyAsync(ps => ps.StatusName.ToLower() == statusName.ToLower());
        }
    }
}