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
    public class VoucherUsageRepository : BaseRepository<VoucherUsage>, IVoucherUsageRepository
    {
        private readonly SnapdiDbV2Context _context;
        public VoucherUsageRepository(SnapdiDbV2Context context) : base(context)
        {
            _context = context;
        }

        public Task ApplyVoucher(int userId, int bookingId, int voucherId)
        {
            throw new NotImplementedException();
        }

        public async Task<VoucherUsage> GetByUserId(int userId)
        {
            return await _context.VoucherUsages.FirstOrDefaultAsync(vu => vu.UserId == userId);
        }

        public async Task<VoucherUsage> GetByVoucherId(int voucherId)
        {
            return await _context.VoucherUsages.FirstOrDefaultAsync(vu => vu.VoucherId == voucherId);
        }

        public async Task<bool> HasUserUsedVoucherAsync(int voucherId, int userId)
        {
            return await _context.VoucherUsages.AnyAsync(vu => vu.VoucherId == voucherId && vu.UserId == userId);
        }
        public async Task<int> CountByVoucherId(int voucherId)
        {
            return await _context.VoucherUsages.CountAsync(vu => vu.VoucherId == voucherId);
        }
    }
}
