using Snapdi.Repositories.Interfaces;
using Snapdi.Repositories.Models;
using Snapdi.Services.Interfaces;

namespace Snapdi.Services.Services
{
    public class VoucherUsageService : IVoucherUsageService
    {
        private readonly IVoucherUsageRepository _voucherUsageRepository;
        private readonly IVoucherRepository _voucherRepository;
        public VoucherUsageService(IVoucherUsageRepository voucherUsageRepository, IVoucherRepository voucherRepository)
        {
            _voucherUsageRepository = voucherUsageRepository;
            _voucherRepository = voucherRepository;
        }

        public async Task AddAsync(VoucherUsage voucherUsage)
        {
            await _voucherUsageRepository.AddAsync(voucherUsage);
        }

        public async Task ApplyVoucher(int userId, int bookingId, int voucherId)
        {
            var voucher = await _voucherRepository.GetByIdAsync(voucherId);

            if(voucher == null || !voucher.IsActive)
            {
                throw new Exception("Invalid or inactive voucher.");
            }
            
            var date = DateTime.UtcNow;

            if(voucher.StartDate < date || voucher.EndDate > date)
            {
                throw new Exception("Voucher is not valid at this time.");
            }

            var totalUsages = await CountAsync(voucherId);

            if(voucher.UsageLimit.HasValue && totalUsages >= voucher.UsageLimit.Value)
            {
                throw new Exception("Voucher usage limit reached.");
            }

            var hasUsed = await _voucherUsageRepository.HasUserUsedVoucherAsync(voucherId, userId);

            if(hasUsed)
            {
                throw new Exception("User has already used this voucher.");
            }

            var voucherUsage = new VoucherUsage
            {
                UserId = userId,
                BookingId = bookingId,
                VoucherId = voucherId,
                UsedAt = date
            };

            await _voucherUsageRepository.AddAsync(voucherUsage);
        }

        public async Task<int> CountAsync(int voucherId)
        {
            return await _voucherUsageRepository.CountByVoucherId(voucherId);
        }

        public async Task<bool> HasUserUsedVoucherAsync(int userId, int voucherId)
        {
            return await _voucherUsageRepository.HasUserUsedVoucherAsync(voucherId, userId);
        }
    }
}
