using Microsoft.EntityFrameworkCore;
using TicketingSystem.Application.Interfaces.persistence;
using TicketingSystem.Domain.Entities;
using TicketingSystem.Domain.QueryFilters;
using TicketingSystem.Infrastructure.Data;

namespace TicketingSystem.Infrastructure.Repositories;

public class EventRepository : IEventRepository
{
    private readonly ApplicationDbContext _context;

    public EventRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Event>> GetAllAsync(EventFilter filter, CancellationToken cancellationToken)
    {
        IQueryable<Event> query = _context.Events;

        if (!string.IsNullOrEmpty(filter.Name))
            query = query.Where(e => e.Name.Contains(filter.Name));

        if (!string.IsNullOrEmpty(filter.Status))
            query = query.Where(e => e.Status == filter.Status);

        if (!string.IsNullOrEmpty(filter.Venue))
            query = query.Where(e => e.Venue.Contains(filter.Venue));

        query = query.ApplyPaging(filter.Page, filter.Take);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<Event?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _context.Events.FirstOrDefaultAsync(e => e.Id == id, ct);
    }

    public async Task AddAsync(Event @event, CancellationToken ct = default)
    {
        await _context.Events.AddAsync(@event, ct);
    }

    public async Task UpdateAsync(Event @event, CancellationToken ct = default)
    {
        _context.Events.Update(@event);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var @event = await GetByIdAsync(id, ct);
        if (@event != null)
        {
            _context.Events.Remove(@event);
        }
    }
}
