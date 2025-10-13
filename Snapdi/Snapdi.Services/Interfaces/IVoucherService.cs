using Snapdi.Repositories.Models;
using Snapdi.Services.DTOs.RequestModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snapdi.Services.Interfaces
{
    public interface IVoucherService
    {
        Task<IEnumerable<Voucher>> GetActiveVouchersAsync();
        Task<Voucher> GetByCodeAsync(string code);
        Task<bool> IsCodeExistsAsync(string code);
        Task<Voucher> GetByIdAsync(int id);
        Task CreateAsync(CreateVoucherDto dto);
        Task UpdateAsync(Voucher voucher);
        Task DeleteAsync(int voucherId);
    }
}
