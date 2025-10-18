using Microsoft.EntityFrameworkCore;
using Snapdi.Repositories.Context;
using Snapdi.Repositories.Interfaces;
using Snapdi.Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snapdi.Repositories.Repositories
{
    public class VoucherRepository : BaseRepository<Voucher>, IVoucherRepository
    {
        private readonly SnapdiDbV2Context _context;
        public VoucherRepository(SnapdiDbV2Context context) : base(context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Voucher>> GetActiveVouchersAsync()
        {
            return await _context.Vouchers
                .Where(v => v.IsActive && (v.EndDate == null || v.EndDate > DateTime.UtcNow))
                .ToListAsync();
        }

        public async Task<Voucher?> GetByCodeAsync(string code)
        {
            return await _context.Vouchers
                .FirstOrDefaultAsync(v => v.Code == code);
        }

        public Task<bool> IsCodeExistsAsync(string code)
        {
            return _context.Vouchers
                .AnyAsync(v => v.Code == code);
        }
    }
}
