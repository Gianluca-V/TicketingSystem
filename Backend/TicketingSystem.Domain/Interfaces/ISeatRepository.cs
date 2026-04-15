using TicketingSystem.Domain.Entities;

namespace TicketingSystem.Domain.Interfaces;

public interface ISeatRepository
{
    Task<Seat?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task AddAsync(Seat seat, CancellationToken cancellationToken = default);
    Task UpdateAsync(Seat seat, CancellationToken cancellationToken = default);
}
