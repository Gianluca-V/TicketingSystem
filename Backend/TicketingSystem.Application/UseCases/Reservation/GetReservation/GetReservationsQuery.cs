using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicketingSystem.Application.DTOs;
using TicketingSystem.Application.Interfaces.Services;

namespace TicketingSystem.Application.UseCases.Reservation.GetReservation
{
    public class GetReservationsQuery : IQuery<IEnumerable<ReservationDto>>
    {
        public string? UserId { get; set; }
        public int? SeatId { get; set; }
        public bool? Expired { get; set; }

        public int Page { get; set; } = 1;
        public int Take { get; set; } = 100;
    }
}
