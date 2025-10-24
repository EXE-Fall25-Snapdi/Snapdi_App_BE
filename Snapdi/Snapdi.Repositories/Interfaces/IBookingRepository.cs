using Snapdi.Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snapdi.Repositories.Interfaces
{
    public interface IBookingRepository : IBaseRepository<Models.Booking>
    {
        Task<Booking?> GetBookingWithDetailsAsync(int id);
        // New: get all bookings for a user (as customer or photographer)
        Task<IEnumerable<Booking>> GetBookingsForUserAsync(int userId);
    }
}
