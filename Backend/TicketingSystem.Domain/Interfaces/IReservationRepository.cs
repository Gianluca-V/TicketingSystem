using TicketingSystem.Domain.Entities;

namespace TicketingSystem.Domain.Interfaces;

public interface IReservationRepository
{
    Task<Reservation?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Reservation?> GetActiveAsync(int seatId, CancellationToken cancellationToken = default);
    Task AddAsync(Reservation reservation, CancellationToken cancellationToken = default);
    Task UpdateAsync(Reservation reservation, CancellationToken cancellationToken = default);
    Task DeleteAsync(Reservation reservation, CancellationToken cancellationToken = default);
    Task<IEnumerable<Reservation>> GetExpiredAsync(CancellationToken cancellationToken = default);
}
