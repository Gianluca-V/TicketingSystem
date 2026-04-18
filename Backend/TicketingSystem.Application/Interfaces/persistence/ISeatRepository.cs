using TicketingSystem.Domain.Entities;

namespace TicketingSystem.Application.Interfaces.persistence;

public interface ISeatRepository
{
    Task<Seat?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Seat>> GetBySectorAsync(int sectorId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Seat>> GetAll(CancellationToken cancellation = default);
    Task AddAsync(Seat seat, CancellationToken cancellationToken = default);
    Task UpdateAsync(Seat seat, CancellationToken cancellationToken = default);
    
}
