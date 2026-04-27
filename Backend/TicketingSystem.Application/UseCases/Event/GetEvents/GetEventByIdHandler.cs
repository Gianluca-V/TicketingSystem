using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicketingSystem.Application.DTOs;
using TicketingSystem.Application.Interfaces.persistence;
using TicketingSystem.Application.Interfaces.Services;

namespace TicketingSystem.Application.UseCases.Event.GetEvents
{
    public class GetEventByIdHandler : IQueryHandler<GetEventByIdQuery, EventDto?>
    {
        private readonly IEventRepository _eventRepository;

        public GetEventByIdHandler(IEventRepository eventRepository)
        {
            _eventRepository = eventRepository;
        }

        public async Task<EventDto?> Handle(GetEventByIdQuery query, CancellationToken ct)
        {
            var e = await _eventRepository.GetByIdAsync(query.Id, ct);

            if (e is null)
                return null;

            return new EventDto(
                e.Id,
                e.Name,
                e.EventDate,
                e.Venue,
                0 
            );
        }
    }
}
