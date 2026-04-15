using TicketingSystem.Application.DTOs;

namespace TicketingSystem.Application.Services;

public interface IEventQueryService
{
    Task<IEnumerable<EventDto>> GetEventsAsync(CancellationToken cancellationToken = default);
}
