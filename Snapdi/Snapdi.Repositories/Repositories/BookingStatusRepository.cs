using Microsoft.EntityFrameworkCore;
using Snapdi.Repositories.Context;
using Snapdi.Repositories.Interfaces;
using Snapdi.Repositories.Models;
using System.Threading.Tasks;

namespace Snapdi.Repositories.Repositories
{
    public class BookingStatusRepository : BaseRepository<BookingStatus>, IBookingStatusRepository
    {
        public BookingStatusRepository(SnapdiDbV2Context context) : base(context)
        {
        }

        public async Task<BookingStatus?> GetByNameAsync(string statusName)
        {
            return await _dbSet
                .FirstOrDefaultAsync(s => s.StatusName.ToLower() == statusName.ToLower());
        }
    }
}
