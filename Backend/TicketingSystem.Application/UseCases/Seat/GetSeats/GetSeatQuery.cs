using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicketingSystem.Application.DTOs;
using TicketingSystem.Application.Interfaces.Services;

namespace TicketingSystem.Application.UseCases.Seat.GetSeats
{    
    public class GetSeatsQuery() : IQuery<IEnumerable<SeatDto>>
    {
        public int? EventId { get; set; }
        public int? SectorId { get; set; }
        public int? Take { get; set; } = 100;
        public int? Page { get; set; } = 1;
    }
}
