using TicketingSystem.Application.DTOs;
using TicketingSystem.Application.Interfaces.persistence;
using TicketingSystem.Application.Interfaces.Services;
using TicketingSystem.Domain.QueryFilters;

namespace TicketingSystem.Application.UseCases.Event.GetEvents;

public class GetEventsHandler : IQueryHandler<GetEventsQuery, IEnumerable<EventDto>>
{
    private readonly IEventRepository _eventRepository;

    public GetEventsHandler(IEventRepository eventRepository)
    {
        _eventRepository = eventRepository;
    }

    public async Task<IEnumerable<EventDto>> Handle(GetEventsQuery query, CancellationToken ct)
    {
        var filter = new EventFilter
        {
            Name = query.Name,
            Status = query.Status,
            Venue = query.Venue,
            Page = query.Page,
            Take = query.Take
        };

        var events = await _eventRepository.GetAllAsync(filter, ct);

        return events.Select(e => new EventDto(
            e.Id,
            e.Name,
            e.EventDate,
            e.Venue,
            0 // You might want to include sector count if needed, but for now 0
        ));
    }
}
