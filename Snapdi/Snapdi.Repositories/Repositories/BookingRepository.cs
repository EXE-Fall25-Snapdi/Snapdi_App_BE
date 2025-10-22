using Microsoft.EntityFrameworkCore;
using Snapdi.Repositories.Context;
using Snapdi.Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snapdi.Repositories.Repositories
{
    public class BookingRepository : BaseRepository<Models.Booking>, Interfaces.IBookingRepository
    {
        public BookingRepository(SnapdiDbV2Context context) : base(context) { }

        public async Task<Booking?> GetBookingWithDetailsAsync(int id)
        {
            return await _dbSet
                .Include(b => b.Customer)
                .Include(b => b.Photographer)
                .Include(b => b.Status)
                .FirstOrDefaultAsync(b => b.BookingId == id);
        }
    } 
}

