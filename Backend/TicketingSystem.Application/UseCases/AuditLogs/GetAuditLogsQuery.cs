using TicketingSystem.Application.DTOs;
using TicketingSystem.Application.Interfaces.Services;
using TicketingSystem.Domain.Entities;

namespace TicketingSystem.Application.UseCases.AuditLogs;

public class GetAuditLogsQuery : IQuery<IEnumerable<AuditLogDto>>
{
    public int? UserId { get; set; }
    public AuditAction? Action { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public int Page { get; set; } = 1;
    public int Take { get; set; } = 100;
}
