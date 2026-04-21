using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicketingSystem.Application.Interfaces.Services;

namespace TicketingSystem.Application.UseCases.Seat.ReserveSeat
{
    public record ReserveSeatCommand(int SeatId, string UserId) : ICommand;
}
