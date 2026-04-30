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
        private readonly ICacheService _cacheService;

        public GetEventByIdHandler(IEventRepository eventRepository, ICacheService cacheService)
        {
            _eventRepository = eventRepository;
            _cacheService = cacheService;
        }

        public async Task<EventDto?> Handle(GetEventByIdQuery query, CancellationToken ct)
        {
            var cacheKey = $"Event:{query.Id}";
            var cachedEvent = await _cacheService.GetAsync<EventDto>(cacheKey, ct);
            if (cachedEvent != null)
            {
                return cachedEvent;
            }

            var e = await _eventRepository.GetByIdAsync(query.Id, ct);

            if (e is null)
                return null;

            var result = new EventDto(
                e.Id,
                e.Name,
                e.EventDate,
                e.Venue,
                0 
            );

            await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(10), ct);

            return result;
        }
    }
}
