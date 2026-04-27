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
    private readonly ICommandHandler<UpdateSectorCommand> _updateSectorHandler;
    private readonly ICommandHandler<DeleteSectorCommand> _deleteSectorHandler;
    private readonly IQueryHandler<GetSectorsQuery, IEnumerable<SectorDto>> _getSectorsHandler;
    private readonly IQueryHandler<GetSectorByIdQuery, SectorDto?> _getSectorByIdHandler;
    public SectorsController(
        ICommandHandler<CreateSectorCommand, int> createSectorHandler,
        ICommandHandler<UpdateSectorCommand> updateSectorHandler,
        ICommandHandler<DeleteSectorCommand> deleteSectorHandler,
        IQueryHandler<GetSectorsQuery, IEnumerable<SectorDto>> getSectorsHandler,
        IQueryHandler<GetSectorByIdQuery, SectorDto?> getSectorByIdHandler)
    {
        _createSectorHandler = createSectorHandler;
        _updateSectorHandler = updateSectorHandler;
        _deleteSectorHandler = deleteSectorHandler;
        _getSectorsHandler = getSectorsHandler;
        _getSectorByIdHandler = getSectorByIdHandler;
    }

    /// <summary>
    /// List all sectors for a specific event
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<SectorDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSectors(int eventId, [FromQuery] string? name, [FromQuery] int page = 1, [FromQuery] int take = 100, CancellationToken cancellationToken = default)
    {
        var sectors = await _getSectorsHandler.Handle(new GetSectorsQuery 
        { 
            EventId = eventId,
            Name = name,
            Page = page,
            Take = take
        }, cancellationToken);
        return Ok(sectors);
    }

    /// <summary>
    /// Get sector by id for a specific event
    /// </summary>
    [HttpGet("{sectorId}")]
    [ProducesResponseType(typeof(SectorDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int eventId, int sectorId, CancellationToken cancellationToken)
    {
        var result = await _getSectorByIdHandler.Handle(
            new GetSectorByIdQuery
            {
                EventId = eventId,
                SectorId = sectorId
            },
            cancellationToken
        );

        if (result is null)
            return NotFound();

        return Ok(result);
    }

    /// <summary>
    /// Create a new sector for an event
    /// </summary>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(int eventId, [FromBody] CreateSectorCommand command, CancellationToken cancellationToken)
    {
        command.EventId = eventId;
        var sectorId = await _createSectorHandler.Handle(command, cancellationToken);
        return CreatedAtAction(nameof(GetById),new { eventId = eventId, sectorId = sectorId },new { Id = sectorId });
    }

    /// <summary>
    /// Update an existing sector
    /// </summary>
    [HttpPut("{sectorId}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int eventId, int sectorId, [FromBody] UpdateSectorCommand command, CancellationToken cancellationToken)
    {
        command.Id = sectorId;
        await _updateSectorHandler.Handle(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Delete a sector
    /// </summary>
    [HttpDelete("{sectorId}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int eventId, int sectorId, CancellationToken cancellationToken)
    {
        await _deleteSectorHandler.Handle(new DeleteSectorCommand { Id = sectorId }, cancellationToken);
        return NoContent();
    }
}
