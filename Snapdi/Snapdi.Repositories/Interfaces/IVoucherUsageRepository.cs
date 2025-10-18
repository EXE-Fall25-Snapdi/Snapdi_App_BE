using Snapdi.Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snapdi.Repositories.Interfaces
{
    public interface IVoucherUsageRepository : IBaseRepository<VoucherUsage>
    {
        Task<bool> HasUserUsedVoucherAsync(int voucherId, int userId);
        Task ApplyVoucher(int userId, int bookingId, int voucherId);
        Task<VoucherUsage> GetByVoucherId(int voucherId);
        Task<VoucherUsage> GetByUserId(int userId);
        Task<int> CountByVoucherId(int voucherId);
    }
}
