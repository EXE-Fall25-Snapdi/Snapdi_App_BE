using Snapdi.Repositories.Interfaces;
using Snapdi.Repositories.Models;
using Snapdi.Services.DTOs.RequestModels;
using Snapdi.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snapdi.Services.Services
{
    public class VoucherService : IVoucherService
    {
        private readonly IVoucherRepository _voucherRepository;
        public VoucherService(IVoucherRepository voucherRepository)
        {
            _voucherRepository = voucherRepository;
        }
        public async Task CreateAsync(CreateVoucherDto dto)
        {
            var existingVoucher = await _voucherRepository.GetByCodeAsync(dto.Code);

            if (existingVoucher != null)
            {
                throw new Exception("Voucher code already exists.");
            }

            if (dto.StartDate < DateTime.UtcNow || dto.EndDate < dto.StartDate)
            {
                throw new Exception("Voucher dates are not valid.");
            }

            var voucher = new Voucher
            {
                Code = dto.Code,
                Description = dto.Description,
                DiscountType = dto.DiscountType,
                DiscountValue = dto.DiscountValue,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                UsageLimit = dto.UsageLimit,
                IsActive = true
            };

            await _voucherRepository.AddAsync(voucher);

            await _voucherRepository.SaveChangesAsync();
        }

        public async Task DeleteAsync(int voucherId)
        {
            await _voucherRepository.DeleteByIdAsync(voucherId);
        }

        public async Task<IEnumerable<Voucher>> GetActiveVouchersAsync()
        {
            return await _voucherRepository.GetActiveVouchersAsync();
        }

        public async Task<Voucher?> GetByCodeAsync(string code)
        {
            return await _voucherRepository.GetByCodeAsync(code);
        }

        public async Task<Voucher> GetByIdAsync(int id)
        {
            return await _voucherRepository.GetByIdAsync(id);
        }

        public async Task<bool> IsCodeExistsAsync(string code)
        {
            return await _voucherRepository.IsCodeExistsAsync(code);
        }

        public async Task UpdateAsync(Voucher voucher)
        {
            await _voucherRepository.UpdateAsync(voucher);
        }
    }
}
