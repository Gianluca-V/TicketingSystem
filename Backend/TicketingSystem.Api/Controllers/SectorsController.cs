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
    private readonly ICommandHandler<CreateSectorCommand, int> _createSectorHandler;
    private readonly IQueryHandler<GetSectorsQuery, IEnumerable<SectorDto>> _getSectorsHandler;
    private readonly IQueryHandler<GetSectorByIdQuery, SectorDto?> _getSectorByIdHandler;

    public SectorsController(
        ICommandHandler<CreateSectorCommand, int> createSectorHandler,
        IQueryHandler<GetSectorsQuery, IEnumerable<SectorDto>> getSectorsHandler,
        IQueryHandler<GetSectorByIdQuery, SectorDto?> getSectorByIdHandler)
    {
        _createSectorHandler = createSectorHandler;
        _getSectorsHandler = getSectorsHandler;
        _getSectorByIdHandler = getSectorByIdHandler;
    }

    [HttpPost]
    public async Task<IActionResult> Create(int eventId, [FromBody] CreateSectorCommand command, CancellationToken ct)
    {
        command.EventId = eventId;
        var sectorId = await _createSectorHandler.Handle(command, ct);

        return CreatedAtAction(nameof(GetSectorById), new { eventId, id = sectorId }, new SectorDto(
            sectorId,
            command.EventId,
            string.Empty,
            command.Name,
            command.Price,
            command.Capacity
        ));
    }

    [HttpGet]
    public async Task<IActionResult> GetSectors(int eventId, [FromQuery] GetSectorsQuery query, CancellationToken ct)
    {
        query.EventId = eventId;

        var sectors = await _getSectorsHandler.Handle(query, ct);
        return Ok(sectors);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetSectorById(int eventId, int id, CancellationToken ct)
    {
        var sectorDto = await _getSectorByIdHandler.Handle(new GetSectorByIdQuery
        {
            EventId = eventId,
            SectorId = id
        }, ct);

        if (sectorDto is null)
            return NotFound();

        return Ok(sectorDto);
    }
}
