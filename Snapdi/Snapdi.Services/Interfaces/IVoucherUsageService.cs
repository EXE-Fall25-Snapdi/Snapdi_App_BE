using Snapdi.Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snapdi.Services.Interfaces
{
    public interface IVoucherUsageService
    {
        Task ApplyVoucher(int userId, int bookingId, string code);
        Task<bool> HasUserUsedVoucherAsync(int userId, int voucherId);
        Task AddAsync(VoucherUsage voucherUsage);
        Task<int> CountAsync(int voucherId);
    }
}
