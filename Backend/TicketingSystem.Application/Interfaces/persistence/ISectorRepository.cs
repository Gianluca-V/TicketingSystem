using TicketingSystem.Domain.Entities;
using TicketingSystem.Domain.QueryFilters;

namespace TicketingSystem.Application.Interfaces.persistence;

public interface ISectorRepository
{
    Task<Sector?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<Sector>> GetAllAsync(SectorFilter filter, CancellationToken ct = default);
    Task AddAsync(Sector sector, CancellationToken ct = default);
    Task UpdateAsync(Sector sector, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
