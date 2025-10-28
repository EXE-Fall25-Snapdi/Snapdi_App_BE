using Microsoft.EntityFrameworkCore;
using Snapdi.Repositories.Context;
using Snapdi.Repositories.Interfaces;
using Snapdi.Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Snapdi.Repositories.Repositories
{
    public class PaymentRepository : BaseRepository<Payment>, IPaymentRepository
    {
        public PaymentRepository(SnapdiDbV2Context context) : base(context)
        {
        }

        /// <summary>
        /// Get all payments within a specific date range, including only successful/completed payments
        /// </summary>
        public async Task<IEnumerable<Payment>> GetPaymentsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Payments
                .Include(p => p.PaymentStatus)
                .Include(p => p.Booking)
                .Where(p => p.PaymentDate >= startDate && p.PaymentDate <= endDate)
                .Where(p => p.PaymentStatus != null && 
                           (p.PaymentStatus.StatusName.ToLower() == "done" ||
                            p.PaymentStatus.StatusName.ToLower() == "paid" ||
                            p.PaymentStatus.StatusName.ToLower() == "completed" ||
                            p.PaymentStatus.StatusName.ToLower() == "success" ||
                            p.PaymentStatus.StatusName.ToLower() == "confirmed"))
                .OrderBy(p => p.PaymentDate)
                .ToListAsync();
        }
    }
}

