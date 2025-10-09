using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Snapdi.Repositories.Models;
using Snapdi.Services.Interfaces;

namespace Snapdi.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VoucherUsagesController : ControllerBase
    {
        private readonly IVoucherUsageService _voucherUsageService;
        public VoucherUsagesController(IVoucherUsageService voucherUsageService)
        {
            _voucherUsageService = voucherUsageService;
        }

        [HttpGet("count")]
        public async Task<ActionResult<int>> CountVoucherUsages(int voucherId)
        {
            try
            {
                var count = await _voucherUsageService.CountAsync(voucherId);
                return Ok(count);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("apply")]
        public async Task<IActionResult> ApplyVoucher(int userId, int bookingId, int voucherId)
        {
            try
            {
                await _voucherUsageService.ApplyVoucher(userId, bookingId, voucherId);
                return Ok("Voucher applied successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error applying voucher: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddVoucherUsage([FromBody] VoucherUsage voucherUsage)
        {
            try
            {
                //var voucherUsage = new VoucherUsage
                //{
                //    UserId = voucherUsageDto.UserId,
                //    BookingId = voucherUsageDto.BookingId,
                //    VoucherId = voucherUsageDto.VoucherId,
                //    UsedAt = DateTime.UtcNow
                //};
                await _voucherUsageService.AddAsync(voucherUsage);
                return Ok("Voucher usage recorded successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
