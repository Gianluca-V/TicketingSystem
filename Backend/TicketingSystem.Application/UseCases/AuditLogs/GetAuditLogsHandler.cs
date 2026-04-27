using TicketingSystem.Application.DTOs;
using TicketingSystem.Application.Interfaces.persistence;
using TicketingSystem.Application.Interfaces.Services;
using TicketingSystem.Domain.QueryFilters;

namespace TicketingSystem.Application.UseCases.AuditLogs;

public class GetAuditLogsHandler : IQueryHandler<GetAuditLogsQuery, IEnumerable<AuditLogDto>>
{
    private readonly IAuditRepository _auditRepository;

    public GetAuditLogsHandler(IAuditRepository auditRepository)
    {
        _auditRepository = auditRepository;
    }

    public async Task<IEnumerable<AuditLogDto>> Handle(GetAuditLogsQuery query, CancellationToken cancellationToken)
    {
        var filter = new AuditFilter
        {
            UserId = query.UserId?.ToString(),
            Action = query.Action,
            From = query.From,
            To = query.To,
            Page = query.Page,
            Take = query.Take
        };

        var logs = await _auditRepository.GetAllAsync(filter, cancellationToken);

        return logs.Select(l => new AuditLogDto(
            l.Id,
            l.UserId,
            l.Action.ToString(),
            l.ResourceType,
            l.ResourceId,
            l.Details,
            l.OccurredAt
        ));
    }
}
