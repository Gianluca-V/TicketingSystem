using TicketingSystem.Domain.Entities;
using TicketingSystem.Domain.QueryFilters;

namespace TicketingSystem.Application.Interfaces.persistence;

public interface IEventRepository
{
    Task<Event?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<Event>> GetAllAsync(EventFilter filter, CancellationToken ct = default);
    Task AddAsync(Event @event, CancellationToken ct = default);
    Task UpdateAsync(Event @event, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
