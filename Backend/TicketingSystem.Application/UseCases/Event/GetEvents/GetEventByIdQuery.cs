using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicketingSystem.Application.DTOs;
using TicketingSystem.Application.Interfaces.Services;

namespace TicketingSystem.Application.UseCases.Event.GetEvents
{
    public class GetEventByIdQuery : IQuery<EventDto?>
    {
        public int Id { get; set; }
    }
}
