using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TicketingSystem.Application.DTOs;
using TicketingSystem.Application.Interfaces.Services;
using TicketingSystem.Application.UseCases.AuditLogs;

namespace TicketingSystem.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class AuditLogsController : ControllerBase
{
    private readonly IQueryHandler<GetAuditLogsQuery, IEnumerable<AuditLogDto>> _getAuditLogsHandler;

    public AuditLogsController(IQueryHandler<GetAuditLogsQuery, IEnumerable<AuditLogDto>> getAuditLogsHandler)
    {
        _getAuditLogsHandler = getAuditLogsHandler;
    }

    /// <summary>
    /// List audit logs (Admin)
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AuditLogDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAuditLogs([FromQuery] GetAuditLogsQuery query, CancellationToken cancellationToken)
    {
        var logs = await _getAuditLogsHandler.Handle(query, cancellationToken);
        return Ok(logs);
    }
}
