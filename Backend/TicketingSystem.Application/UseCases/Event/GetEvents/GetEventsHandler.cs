using TicketingSystem.Application.DTOs;
using TicketingSystem.Application.Interfaces.persistence;
using TicketingSystem.Application.Interfaces.Services;
using TicketingSystem.Domain.QueryFilters;

namespace TicketingSystem.Application.UseCases.Event.GetEvents;

public class GetEventsHandler : IQueryHandler<GetEventsQuery, IEnumerable<EventDto>>
{
    private readonly IEventRepository _eventRepository;
    private readonly ICacheService _cacheService;

    public GetEventsHandler(IEventRepository eventRepository, ICacheService cacheService)
    {
        _eventRepository = eventRepository;
        _cacheService = cacheService;
    }

    public async Task<IEnumerable<EventDto>> Handle(GetEventsQuery query, CancellationToken ct)
    {
        var cacheKey = $"Events:List:{query.Name}:{query.Status}:{query.Venue}:{query.Page}:{query.Take}";
        
        var cachedEvents = await _cacheService.GetAsync<IEnumerable<EventDto>>(cacheKey, ct);
        if (cachedEvents != null)
        {
            return cachedEvents;
        }

        var filter = new EventFilter
        {
            Name = query.Name,
            Status = query.Status,
            Venue = query.Venue,
            Page = query.Page,
            Take = query.Take
        };

        var events = await _eventRepository.GetAllAsync(filter, ct);

        var result = events.Select(e => new EventDto(
            e.Id,
            e.Name,
            e.EventDate,
            e.Venue,
            0 
        )).ToList();

        await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(10), ct);

        return result;
    }
}
