using TicketingSystem.Domain.Entities;

namespace TicketingSystem.Domain.Interfaces;

public interface IAuditRepository
{
    Task AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default);
    Task<IEnumerable<AuditLog>> GetAllAsync(CancellationToken cancellationToken = default);
}
