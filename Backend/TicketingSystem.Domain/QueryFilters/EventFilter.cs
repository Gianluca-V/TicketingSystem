using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketingSystem.Domain.QueryFilters
{
    public class EventFilter
    {
        public string? Name { get; set; }
        public string? Status { get; set; }
        public string? Venue { get; set; }
        public int? Take { get; set; } = 100;
        public int? Page { get; set; } = 1;
    }
}
