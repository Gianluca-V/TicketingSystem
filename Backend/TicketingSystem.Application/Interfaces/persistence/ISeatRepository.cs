using TicketingSystem.Domain.QueryFilters;
using TicketingSystem.Domain.Entities;

namespace TicketingSystem.Application.Interfaces.persistence;

public interface ISeatRepository
{
    Task<Seat?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Seat>> GetAllAsync(SeatFilter filter, CancellationToken cancellation = default);
    Task AddAsync(Seat seat, CancellationToken cancellationToken = default);
    Task AddBulkAsync(IEnumerable<Seat> seats, CancellationToken cancellationToken = default);
    Task UpdateAsync(Seat seat, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
