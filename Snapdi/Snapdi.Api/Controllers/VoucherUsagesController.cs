using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Snapdi.Repositories.Models;
using Snapdi.Services.Interfaces;
using System.Security.Claims;

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
        public async Task<IActionResult> ApplyVoucher(int bookingId, string code)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Unauthorized("User ID claim is missing or invalid.");
                }
                await _voucherUsageService.ApplyVoucher(userId, bookingId, code);
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
