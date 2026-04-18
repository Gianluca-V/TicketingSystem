using TicketingSystem.Application.DTOs;

namespace TicketingSystem.Application.Interfaces.Services;

public interface IEventQueryService
{
    Task<IEnumerable<EventDto>> GetEventsAsync(CancellationToken cancellationToken = default);
}
