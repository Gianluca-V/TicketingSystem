using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TicketingSystem.Application.DTOs;
using TicketingSystem.Application.Interfaces.Services;
using TicketingSystem.Application.UseCases.Sector.CreateSector;
using TicketingSystem.Application.UseCases.Sector.DeleteSector;
using TicketingSystem.Application.UseCases.Sector.GetSectors;
using TicketingSystem.Application.UseCases.Sector.UpdateSector;

namespace TicketingSystem.Api.Controllers;

[ApiController]
[Route("api/v1/events/{eventId}/[controller]")]
public class SectorsController : ControllerBase
{
    private readonly IQueryHandler<GetSectorsQuery, IEnumerable<SectorDto>> _getSectorsHandler;

    public SectorsController(
        IQueryHandler<GetSectorsQuery, IEnumerable<SectorDto>> getSectorsHandler)
    {
        _getSectorsHandler = getSectorsHandler;
    }

    [HttpGet]
    public async Task<IActionResult> GetSectors(int eventId, [FromQuery] GetSectorsQuery query, CancellationToken ct)
    {
        query.EventId = eventId;

        var sectors = await _getSectorsHandler.Handle(query, ct);
        return Ok(sectors);
    }
}
