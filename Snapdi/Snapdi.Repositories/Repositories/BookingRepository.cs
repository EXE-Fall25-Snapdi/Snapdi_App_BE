using Microsoft.EntityFrameworkCore;
using Snapdi.Repositories.Context;
using Snapdi.Repositories.Interfaces;
using Snapdi.Repositories.Models;

namespace Snapdi.Repositories.Repositories
{
    public class BookingRepository : BaseRepository<Booking>, IBookingRepository
    {
        public BookingRepository(SnapdiDbV2Context context) : base(context)
        {
        }

        public async Task<Booking?> GetBookingWithDetailsAsync(int bookingId)
        {
            return await _context.Bookings
                .Include(b => b.Customer)
                    .ThenInclude(c => c!.Role)
                .Include(b => b.Photographer)
                    .ThenInclude(p => p!.Role)
                .Include(b => b.Photographer)
                    .ThenInclude(p => p!.PhotographerProfile)
                        .ThenInclude(pp => pp!.PhotographerPhotoTypes)
                            .ThenInclude(ppt => ppt.PhotoType)
                .Include(b => b.Status)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);
        }

        public async Task<IEnumerable<Booking>> GetBookingsByCustomerAsync(int customerId)
        {
            return await _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.Photographer)
                    .ThenInclude(p => p.PhotographerProfile)
                .Include(b => b.Status)
                .Where(b => b.CustomerId == customerId)
                .OrderByDescending(b => b.ScheduleAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Booking>> GetBookingsByPhotographerAsync(int photographerId)
        {
            return await _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.Photographer)
                    .ThenInclude(p => p!.PhotographerProfile)
                        .ThenInclude(pp => pp!.PhotographerPhotoTypes)
                .Include(b => b.Status)
                .Where(b => b.PhotographerId == photographerId)
                .OrderByDescending(b => b.ScheduleAt)
                .ToListAsync();
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

        public async Task<IEnumerable<Booking>> GetBookingsByStatusAsync(int statusId)
        {
            return await _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.Photographer)
                .Include(b => b.Status)
                .Where(b => b.StatusId == statusId)
                .OrderByDescending(b => b.ScheduleAt)
                .ToListAsync();
        }

        public async Task<(IEnumerable<Booking> Bookings, int TotalCount)> SearchBookingsAsync(
            int page,
            int pageSize,
            string? searchTerm = null,
            int? customerId = null,
            int? photographerId = null,
            int? statusId = null,
            string? locationAddress = null,
            double? minPrice = null,
            double? maxPrice = null,
            DateTime? scheduleFrom = null,
            DateTime? scheduleTo = null,
            string? sortBy = "scheduleAt",
            string? sortDirection = "desc")
        {
            var query = _context.Bookings
                .Include(b => b.Customer)
                    .ThenInclude(c => c!.Role)
                .Include(b => b.Photographer)
                    .ThenInclude(p => p!.Role)
                .Include(b => b.Status)
                .AsQueryable();

            // Apply search term filter
            if (!string.IsNullOrEmpty(searchTerm))
            {
                var searchLower = searchTerm.ToLower();
                query = query.Where(b =>
                    (b.Customer != null && b.Customer.Name.ToLower().Contains(searchLower)) ||
                    (b.Photographer != null && b.Photographer.Name.ToLower().Contains(searchLower)) ||
                    (b.LocationAddress != null && b.LocationAddress.ToLower().Contains(searchLower)));
            }

            // Apply customer filter
            if (customerId.HasValue)
            {
                query = query.Where(b => b.CustomerId == customerId.Value);
            }

            // Apply photographer filter
            if (photographerId.HasValue)
            {
                query = query.Where(b => b.PhotographerId == photographerId.Value);
            }

            // Apply status filter
            if (statusId.HasValue)
            {
                query = query.Where(b => b.StatusId == statusId.Value);
            }

            // Apply location filter
            if (!string.IsNullOrEmpty(locationAddress))
            {
                query = query.Where(b => b.LocationAddress != null &&
                    b.LocationAddress.ToLower().Contains(locationAddress.ToLower()));
            }

            // Apply price range filters
            if (minPrice.HasValue)
            {
                query = query.Where(b => b.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(b => b.Price <= maxPrice.Value);
            }

            // Apply schedule date range filters
            if (scheduleFrom.HasValue)
            {
                query = query.Where(b => b.ScheduleAt >= scheduleFrom.Value);
            }

            if (scheduleTo.HasValue)
            {
                query = query.Where(b => b.ScheduleAt <= scheduleTo.Value);
            }

            // Apply sorting
            var isDescending = sortDirection?.ToLower() == "desc";

            if (!string.IsNullOrEmpty(sortBy))
            {
                query = sortBy.ToLower() switch
                {
                    "scheduleat" => isDescending
                        ? query.OrderByDescending(b => b.ScheduleAt)
                        : query.OrderBy(b => b.ScheduleAt),
                    "price" => isDescending
                        ? query.OrderByDescending(b => b.Price)
                        : query.OrderBy(b => b.Price),
                    "customername" => isDescending
                        ? query.OrderByDescending(b => b.Customer != null ? b.Customer.Name : "")
                        : query.OrderBy(b => b.Customer != null ? b.Customer.Name : ""),
                    "photographername" => isDescending
                        ? query.OrderByDescending(b => b.Photographer != null ? b.Photographer.Name : "")
                        : query.OrderBy(b => b.Photographer != null ? b.Photographer.Name : ""),
                    "statusname" => isDescending
                        ? query.OrderByDescending(b => b.Status != null ? b.Status.StatusName : "")
                        : query.OrderBy(b => b.Status != null ? b.Status.StatusName : ""),
                    _ => isDescending
                        ? query.OrderByDescending(b => b.ScheduleAt)
                        : query.OrderBy(b => b.ScheduleAt)
                };
            }
            else
            {
                query = isDescending
                    ? query.OrderByDescending(b => b.ScheduleAt)
                    : query.OrderBy(b => b.ScheduleAt);
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync();

            // Apply pagination
            var bookings = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (bookings, totalCount);
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

        public async Task UpdateBookingStatusAsync(int bookingId, int statusId)
        {
            var booking = await _context.Bookings.FindAsync(bookingId);
            if (booking != null)
            {
                booking.StatusId = statusId;
            }
        }

        public async Task<bool> BookingExistsAsync(int bookingId)
        {
            return await _context.Bookings.AnyAsync(b => b.BookingId == bookingId);
        }

        public async Task<Dictionary<int, int>> GetBookingsCountByStatusAsync()
        {
            return await _context.Bookings
                .Where(b => b.StatusId != null)
                .GroupBy(b => b.StatusId!.Value)
                .Select(g => new { StatusId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.StatusId, x => x.Count);
        }

        public async Task<List<(int StatusId, string StatusName, int Count)>> GetBookingsCountByStatusWithNamesAsync()
        {
            var result = await _context.Bookings
                .Where(b => b.StatusId != null && b.Status != null)
                .GroupBy(b => new { b.StatusId!.Value, b.Status!.StatusName })
                .Select(g => new 
                { 
                    StatusId = g.Key.Value, 
                    StatusName = g.Key.StatusName, 
                    Count = g.Count() 
                })
                .ToListAsync();

            return result.Select(x => (x.StatusId, x.StatusName, x.Count)).ToList();
        }

        public override async Task<Booking?> GetByIdAsync(int id)
        {
            return await GetBookingWithDetailsAsync(id);
        }

        public override async Task<IEnumerable<Booking>> GetAllAsync()
        {
            return await _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.Photographer)
                .Include(b => b.Status)
                .OrderByDescending(b => b.ScheduleAt)
                .ToListAsync();
        }
    }
}

