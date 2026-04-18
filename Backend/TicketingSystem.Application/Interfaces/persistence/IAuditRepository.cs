using TicketingSystem.Domain.Entities;

namespace TicketingSystem.Application.Interfaces.persistence;

public interface IAuditRepository
{
    Task AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default);
    Task<IEnumerable<AuditLog>> GetAllAsync(CancellationToken cancellationToken = default);
}
