using Snapdi.Repositories.Models;
using System.Threading.Tasks;

namespace Snapdi.Repositories.Interfaces
{
    public interface IBookingStatusRepository : IBaseRepository<BookingStatus>
    {
        Task<BookingStatus?> GetByNameAsync(string statusName);
    }
}
