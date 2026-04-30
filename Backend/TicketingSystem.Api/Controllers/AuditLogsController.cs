using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TicketingSystem.Application.DTOs;
using TicketingSystem.Application.Interfaces.Services;
using TicketingSystem.Application.UseCases.AuditLogs;

using TicketingSystem.Application.UseCases.AuditLogs;

namespace TicketingSystem.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class AuditLogsController : ControllerBase
{
    private readonly IQueryHandler<GetAuditLogsQuery, IEnumerable<AuditLogDto>> _getAuditLogsHandler;

    public AuditLogsController(
        IQueryHandler<GetAuditLogsQuery, IEnumerable<AuditLogDto>> getAuditLogsHandler)
    {
        _getAuditLogsHandler = getAuditLogsHandler;
    }

    [HttpGet]
    public async Task<IActionResult> GetAuditLogs([FromQuery] GetAuditLogsQuery query, CancellationToken ct)
    {
        var logs = await _getAuditLogsHandler.Handle(query, ct);
        return Ok(logs);
    }
}
