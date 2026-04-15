using Microsoft.EntityFrameworkCore;
using TicketingSystem.Application.DTOs;
using TicketingSystem.Application.Services;
using TicketingSystem.Infrastructure.Data;

namespace TicketingSystem.Infrastructure.Services;

public class EventQueryService : IEventQueryService
{
    private readonly ApplicationDbContext _context;

    public EventQueryService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<EventDto>> GetEventsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Events
            .Select(e => new EventDto(
                e.Id,
                e.Name,
                e.Date,
                e.Sectors.Count))
            .ToListAsync(cancellationToken);
    }
}
