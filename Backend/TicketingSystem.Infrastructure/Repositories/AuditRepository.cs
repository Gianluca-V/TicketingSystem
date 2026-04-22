using Microsoft.EntityFrameworkCore;
using TicketingSystem.Application.Interfaces.persistence;
using TicketingSystem.Domain.Entities;
using TicketingSystem.Domain.QueryFilters;
using TicketingSystem.Infrastructure.Data;

using TicketingSystem.Application.Interfaces.Services;

namespace TicketingSystem.Infrastructure.Repositories;

public class AuditRepository : IAuditRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public AuditRepository(ApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default)
    {
        if (auditLog.UserId == 0 && _currentUserService.UserId.HasValue)
        {
            auditLog.UserId = _currentUserService.UserId.Value;
        }
        await _context.AuditLogs.AddAsync(auditLog, cancellationToken);
    }

    public async Task<IEnumerable<AuditLog>> GetAllAsync(AuditFilter filter, CancellationToken cancellationToken)
    {
        IQueryable<AuditLog> query = _context.AuditLogs;

        if (!string.IsNullOrEmpty(filter.UserId))
        {
            if (int.TryParse(filter.UserId, out int userId))
                query = query.Where(a => a.UserId == userId);
        }

        if (filter.Action.HasValue)
            query = query.Where(a => a.Action == filter.Action);

        if (filter.From.HasValue)
            query = query.Where(a => a.OccurredAt >= filter.From);

        if (filter.To.HasValue)
            query = query.Where(a => a.OccurredAt <= filter.To);

        query = query
            .OrderByDescending(a => a.OccurredAt)
            .ApplyPaging(filter.Page, filter.Take);

        return await query.ToListAsync(cancellationToken);
    }
}
