using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snapdi.Services.DTOs.ResponseModels
{
    public class BookingResponse
    {
        public int BookingId { get; set; }
        public string? CustomerName { get; set; }
        public string? PhotographerName { get; set; }
        public DateTime ScheduleAt { get; set; }
        public string? StatusName { get; set; }
        public double Price { get; set; }
    }
}
