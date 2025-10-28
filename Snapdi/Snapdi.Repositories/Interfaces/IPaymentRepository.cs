using Snapdi.Repositories.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Snapdi.Repositories.Interfaces
{
    public interface IPaymentRepository
    {
        /// <summary>
        /// Get all payments within a specific date range
        /// </summary>
        /// <param name="startDate">Start date of the range</param>
        /// <param name="endDate">End date of the range</param>
        /// <returns>List of payments within the date range</returns>
        Task<IEnumerable<Payment>> GetPaymentsByDateRangeAsync(DateTime startDate, DateTime endDate);
    }
}

