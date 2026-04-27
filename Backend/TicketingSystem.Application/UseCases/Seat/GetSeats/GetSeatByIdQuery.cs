using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicketingSystem.Application.DTOs;
using TicketingSystem.Application.Interfaces.Services;

namespace TicketingSystem.Application.UseCases.Seat.GetSeats
{
    public class GetSeatByIdQuery : IQuery<SeatDto?>
    {
        public int EventId { get; set; }
        public int SectorId { get; set; }
        public int SeatId { get; set; }
    }
}
