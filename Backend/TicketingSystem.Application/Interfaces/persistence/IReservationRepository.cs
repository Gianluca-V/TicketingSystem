using TicketingSystem.Domain.Entities;
using TicketingSystem.Domain.QueryFilters;

namespace TicketingSystem.Application.Interfaces.persistence;

public interface IReservationRepository
{
    Task<Reservation?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Reservation>> GetAllAsync(ReservationFilter filter, CancellationToken cancellationToken = default);
    Task AddAsync(Reservation reservation, CancellationToken cancellationToken = default);
    Task UpdateAsync(Reservation reservation, CancellationToken cancellationToken = default);
    Task DeleteAsync(Reservation reservation, CancellationToken cancellationToken = default);
    Task<IEnumerable<Reservation>> GetExpiredAsync(CancellationToken cancellationToken = default);
}
