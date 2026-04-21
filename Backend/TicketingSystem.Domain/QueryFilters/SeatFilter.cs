using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketingSystem.Domain.QueryFilters
{
    public class SeatFilter
    {
        public int? EventId { get; set; }
        public int? SectorId { get; set; }
        public string? Status { get; set; }
        public int? Take { get; set; } = 100;
        public int? Page { get; set; } = 1;
    }
}
