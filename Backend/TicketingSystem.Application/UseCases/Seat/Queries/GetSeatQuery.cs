using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketingSystem.Application.UseCases.Seat.Queries
{    public record GetSeatsQuery(int EventId, int SectorId);
}
