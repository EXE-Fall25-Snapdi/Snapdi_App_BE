using Snapdi.Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snapdi.Repositories.Interfaces
{
    public interface IVoucherRepository : IBaseRepository<Voucher>
    {
        Task<IEnumerable<Voucher>> GetActiveVouchersAsync();
        Task<Voucher?> GetByCodeAsync(string code);
        Task<bool> IsCodeExistsAsync(string code);
    }
}
