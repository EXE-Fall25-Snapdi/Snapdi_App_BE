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
                    .ThenInclude(p => p.PhotographerProfile)
                .Include(b => b.Status)
                .FirstOrDefaultAsync(b => b.BookingId == id);
        }

        public async Task<IEnumerable<Booking>> GetBookingsForUserAsync(int userId)
        {
            return await _dbSet
                .Where(b => b.CustomerId == userId || b.PhotographerId == userId)
                .Include(b => b.Customer)
                .Include(b => b.Photographer)
                    .ThenInclude(p => p.PhotographerProfile)
                .Include(b => b.Status)
                .OrderByDescending(b => b.ScheduleAt)
                .ToListAsync();
        }

        public async Task<(IEnumerable<Booking> Bookings, int TotalCount)> GetBookingsForUserPagedAsync(int userId, int page, int pageSize)
        {
            var query = _dbSet
                .Where(b => b.CustomerId == userId || b.PhotographerId == userId)
                .Include(b => b.Customer)
                .Include(b => b.Photographer)
                    .ThenInclude(p => p.PhotographerProfile)
                .Include(b => b.Status)
                // Sort by BookingId desc as requested
                .OrderByDescending(b => b.BookingId)
                .AsQueryable();

            var total = await query.CountAsync();

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, total);
        }
    } 
}

