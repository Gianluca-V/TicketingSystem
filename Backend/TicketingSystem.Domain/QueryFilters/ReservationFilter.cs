using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketingSystem.Domain.QueryFilters
{
    public class ReservationFilter
    {
        public int? SeatId { get; set; }
        public string? UserId { get; set; }
        public bool? IsActive { get; set; }
        public int? Take { get; set; } = 100;
        public int? Page { get; set; } = 1;
    }
}
