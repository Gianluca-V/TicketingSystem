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
[Route("api/v1/events/{eventId}/sectors/{sectorId}/[controller]")]
public class SeatsController : ControllerBase
{
    private readonly ICommandHandler<CreateSeatCommand, int> _createSeatHandler;
    private readonly ICommandHandler<UpdateSeatCommand> _updateSeatHandler;
    private readonly ICommandHandler<DeleteSeatCommand> _deleteSeatHandler;
    private readonly IQueryHandler<GetSeatsQuery, IEnumerable<SeatDto>> _getSeatsHandler;
    private readonly IQueryHandler<GetSeatByIdQuery, SeatDto?> _getSeatByIdHandler;

    public SeatsController(
        ICommandHandler<CreateSeatCommand, int> createSeatHandler,
        ICommandHandler<UpdateSeatCommand> updateSeatHandler,
        ICommandHandler<DeleteSeatCommand> deleteSeatHandler,
        IQueryHandler<GetSeatsQuery, IEnumerable<SeatDto>> getSeatsHandler,
        IQueryHandler<GetSeatByIdQuery, SeatDto?> getSeatByIdHandler)
    {
        _createSeatHandler = createSeatHandler;
        _updateSeatHandler = updateSeatHandler;
        _deleteSeatHandler = deleteSeatHandler;
        _getSeatsHandler = getSeatsHandler;
        _getSeatByIdHandler = getSeatByIdHandler;
    }

    /// <summary>
    /// List seats for a sector
    /// GET /api/v1/events/{eventId}/sectors/{sectorId}/seats
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<SeatDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        int eventId,
        int sectorId,
        [FromQuery] int page = 1,
        [FromQuery] int take = 100,
        CancellationToken cancellationToken = default)
    {
        var seats = await _getSeatsHandler.Handle(
            new GetSeatsQuery
            {
                EventId = eventId,
                SectorId = sectorId,
                Page = page,
                Take = take
            },
            cancellationToken);

        return Ok(seats);
    }

    /// <summary>
    /// Get seat by id
    /// GET /api/v1/events/{eventId}/sectors/{sectorId}/seats/{seatId}
    /// </summary>
    [HttpGet("{seatId}")]
    [ProducesResponseType(typeof(SeatDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        int eventId,
        int sectorId,
        int seatId,
        CancellationToken cancellationToken)
    {
        var seat = await _getSeatByIdHandler.Handle(
            new GetSeatByIdQuery
            {
                EventId = eventId,
                SectorId = sectorId,
                SeatId = seatId
            },
            cancellationToken);

        if (seat is null)
            return NotFound();

        return Ok(seat);
    }

    /// <summary>
    /// Create seat
    /// POST /api/v1/events/{eventId}/sectors/{sectorId}/seats
    /// </summary>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(
        int eventId,
        int sectorId,
        [FromBody] CreateSeatCommand command,
        CancellationToken cancellationToken)
    {
        command.EventId = eventId;
        command.SectorId = sectorId;

        var seatId = await _createSeatHandler.Handle(command, cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { eventId, sectorId, seatId },
            new { Id = seatId });
    }

    /// <summary>
    /// Update seat
    /// PUT /api/v1/events/{eventId}/sectors/{sectorId}/seats/{seatId}
    /// </summary>
    [HttpPut("{seatId}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        int eventId,
        int sectorId,
        int seatId,
        [FromBody] UpdateSeatCommand command,
        CancellationToken cancellationToken)
    {
        command.Id = seatId;

        await _updateSeatHandler.Handle(command, cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Delete seat
    /// DELETE /api/v1/events/{eventId}/sectors/{sectorId}/seats/{seatId}
    /// </summary>
    [HttpDelete("{seatId}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        int eventId,
        int sectorId,
        int seatId,
        CancellationToken cancellationToken)
    {
        await _deleteSeatHandler.Handle(
            new DeleteSeatCommand
            {
                Id = seatId
            },
            cancellationToken);

        return NoContent();
    }
}