using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicketingSystem.Domain.Entities;

namespace TicketingSystem.Domain.QueryFilters
{
    public class AuditFilter
    {
        public string? UserId { get; set; }
        public AuditAction? Action { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public int? Take { get; set; } = 100;
        public int? Page { get; set; } = 1;
    }
}
