using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snapdi.Services.DTOs.RequestModels
{
    public class CreateBookingRequest
    {
        public int CustomerId { get; set; }
        public int PhotographerId { get; set; }
        public DateTime ScheduleAt { get; set; }
        public string LocationCity { get; set; } = string.Empty;
        public string LocationAddress { get; set; } = string.Empty;
        public int StyleId { get; set; }
        public double Price { get; set; }
    }
}
