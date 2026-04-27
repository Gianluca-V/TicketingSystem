using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TicketingSystem.Application.DTOs;
using TicketingSystem.Application.Interfaces.Services;
using TicketingSystem.Application.UseCases.Seat.CreateSeat;
using TicketingSystem.Application.UseCases.Seat.DeleteSeat;
using TicketingSystem.Application.UseCases.Seat.GetSeats;
using TicketingSystem.Application.UseCases.Seat.UpdateSeat;

namespace TicketingSystem.Api.Controllers;

[ApiController]
public class SeatsController : ControllerBase
{
    private readonly ICommandHandler<CreateSeatCommand, int> _createSeatHandler;
    private readonly ICommandHandler<UpdateSeatCommand> _updateSeatHandler;
    private readonly ICommandHandler<DeleteSeatCommand> _deleteSeatHandler;
    private readonly IQueryHandler<GetSeatsQuery, IEnumerable<SeatDto>> _getSeatsHandler;

    public SeatsController(
        ICommandHandler<CreateSeatCommand, int> createSeatHandler,
        ICommandHandler<UpdateSeatCommand> updateSeatHandler,
        ICommandHandler<DeleteSeatCommand> deleteSeatHandler,
        IQueryHandler<GetSeatsQuery, IEnumerable<SeatDto>> getSeatsHandler)
    {
        _createSeatHandler = createSeatHandler;
        _updateSeatHandler = updateSeatHandler;
        _deleteSeatHandler = deleteSeatHandler;
        _getSeatsHandler = getSeatsHandler;
    }

    /// <summary>
    /// List seats for a specific sector and event
    /// </summary>
    [HttpGet("api/v1/events/{eventId}/sectors/{sectorId}/seats")]
    [ProducesResponseType(typeof(IEnumerable<SeatDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBySector(int eventId, int sectorId, [FromQuery] int page = 1, [FromQuery] int take = 100, CancellationToken cancellationToken = default)
    {
        var seats = await _getSeatsHandler.Handle(new GetSeatsQuery 
        { 
            EventId = eventId,
            SectorId = sectorId,
            Page = page,
            Take = take
        }, cancellationToken);
        return Ok(seats);
    }

    /// <summary>
    /// Create a new seat
    /// </summary>
    [HttpPost("api/v1/seats")]
    [Authorize]
    [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateSeatCommand command, CancellationToken cancellationToken)
    {
        var seatId = await _createSeatHandler.Handle(command, cancellationToken);
        return CreatedAtAction(nameof(GetBySector), new { id = seatId }, new { Id = seatId });
    }

    /// <summary>
    /// Update an existing seat
    /// </summary>
    [HttpPut("api/v1/seats/{id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateSeatCommand command, CancellationToken cancellationToken)
    {
        command.Id = id;
        await _updateSeatHandler.Handle(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Delete a seat
    /// </summary>
    [HttpDelete("api/v1/seats/{id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _deleteSeatHandler.Handle(new DeleteSeatCommand { Id = id }, cancellationToken);
        return NoContent();
    }
}
