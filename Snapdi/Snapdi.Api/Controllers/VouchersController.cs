using Microsoft.AspNetCore.Mvc;
using Snapdi.Repositories.Models;
using Snapdi.Services.DTOs;
using Snapdi.Services.Interfaces;

namespace Snapdi.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VouchersController : ControllerBase
    {
        private readonly IVoucherService _voucherService;

        public VouchersController(IVoucherService voucherService)
        {
            _voucherService = voucherService;
        }

        // GET: api/Vouchers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Voucher>>> GetVouchers()
        {
            var vouchers = await _voucherService.GetActiveVouchersAsync();

            return Ok(vouchers);
        }

        // GET: api/Vouchers/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Voucher>> GetVoucher(int id)
        {
            var voucher = await _voucherService.GetByIdAsync(id);
            return Ok(voucher);
        }

        // PUT: api/Vouchers/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutVoucher(int id, Voucher voucher)
        {
            var existingVoucher = await _voucherService.GetByIdAsync(id);

            if (existingVoucher == null)
            {
                return NotFound();
            }

            if (id != voucher.VoucherId)
            {
                return BadRequest();
            }

            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _voucherService.UpdateAsync(voucher);

            return NoContent();
        }

        // POST: api/Vouchers
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Voucher>> PostVoucher(CreateVoucherDto dto)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _voucherService.CreateAsync(dto);

            return CreatedAtAction("GetVoucher", new { id = dto.Code }, dto);
        }

        // DELETE: api/Vouchers/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVoucher(int id)
        {
            var existingVoucher = await _voucherService.GetByIdAsync(id);
            if (existingVoucher == null)
            {
                return NotFound();
            }
            await _voucherService.DeleteAsync(id);

            return NoContent();
        }

        private bool VoucherExists(int id)
        {
            return (_voucherService.GetByIdAsync(id) != null);
        }
    }
}
