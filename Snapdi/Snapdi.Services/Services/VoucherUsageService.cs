using Snapdi.Repositories.Interfaces;
using Snapdi.Repositories.Models;
using Snapdi.Services.Interfaces;

namespace Snapdi.Services.Services
{
    public class VoucherUsageService : IVoucherUsageService
    {
        private readonly IVoucherUsageRepository _voucherUsageRepository;
        private readonly IVoucherRepository _voucherRepository;
        private readonly IBookingRepository _bookingRepository;
        public VoucherUsageService(
            IVoucherUsageRepository voucherUsageRepository,
            IVoucherRepository voucherRepository, 
            IBookingRepository bookingRepository)
        {
            _voucherUsageRepository = voucherUsageRepository;
            _voucherRepository = voucherRepository;
            _bookingRepository = bookingRepository;
        }

        public async Task AddAsync(VoucherUsage voucherUsage)
        {
            await _voucherUsageRepository.AddAsync(voucherUsage);
            await _voucherUsageRepository.SaveChangesAsync();
        }

        public async Task ApplyVoucher(int userId, int bookingId, string code)
        {
            var voucher = await _voucherRepository.GetByCodeAsync(code);

            if(voucher == null || !voucher.IsActive)
            {
                throw new Exception("Invalid or inactive voucher.");
            }

            var voucherId = voucher.VoucherId;

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

            var booking = await _bookingRepository.GetByIdAsync(bookingId);

            if(booking == null)
            {
                throw new Exception("Booking not existed");
            }

            if(voucher.MinSpend.HasValue && booking.Price < voucher.MinSpend.Value)
            {
                throw new Exception($"Booking total must be at least {voucher.MinSpend.Value} to use this voucher.");
            }

            double discountAmount = 0;

            if (voucher.DiscountType?.ToLower() == "percent")
            {
                discountAmount = booking.Price * (voucher.DiscountValue / 100);
                if (voucher.MaxDiscount.HasValue)
                {
                    discountAmount = Math.Min(discountAmount, voucher.MaxDiscount.Value);
                }
            }
            else if (voucher.DiscountType?.ToLower() == "fixed")
            {
                discountAmount = voucher.DiscountValue;
            }

            discountAmount = Math.Max(0, discountAmount);

            booking.Price -= discountAmount;

            var voucherUsage = new VoucherUsage
            {
                UserId = userId,
                BookingId = bookingId,
                VoucherId = voucherId,
                UsedAt = date
            };

            await _voucherUsageRepository.AddAsync(voucherUsage);
            await _bookingRepository.UpdateAsync(booking);
            await _voucherUsageRepository.SaveChangesAsync();
            await _bookingRepository.SaveChangesAsync();
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
