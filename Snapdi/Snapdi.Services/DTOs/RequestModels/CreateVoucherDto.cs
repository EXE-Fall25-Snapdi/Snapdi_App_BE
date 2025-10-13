using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snapdi.Services.DTOs.RequestModels
{
    public class CreateVoucherDto
    {
        public string Code { get; set; } = null!;

        public string? Description { get; set; }

        public string? DiscountType { get; set; }

        public double DiscountValue { get; set; }

        public double? MaxDiscount { get; set; }

        public double? MinSpend { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public int? UsageLimit { get; set; }
    }
}
