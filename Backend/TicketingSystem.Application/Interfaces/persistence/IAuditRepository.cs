using TicketingSystem.Domain.Entities;
using TicketingSystem.Domain.QueryFilters;

namespace TicketingSystem.Application.Interfaces.persistence;

public interface IAuditRepository
{
    Task AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default);
    Task<IEnumerable<AuditLog>> GetAllAsync(AuditFilter filter, CancellationToken cancellationToken = default);
}
