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
    private readonly IQueryHandler<GetSeatsQuery, IEnumerable<SeatDto>> _getSeatsHandler;
    private readonly IQueryHandler<GetSeatByIdQuery, SeatDto?> _getSeatByIdHandler;

    public SeatsController(
        ICommandHandler<CreateSeatCommand, int> createSeatHandler,
        IQueryHandler<GetSeatsQuery, IEnumerable<SeatDto>> getSeatsHandler,
        IQueryHandler<GetSeatByIdQuery, SeatDto?> getSeatByIdHandler)
    {
        _createSeatHandler = createSeatHandler;
        _getSeatsHandler = getSeatsHandler;
        _getSeatByIdHandler = getSeatByIdHandler;
    }

    [HttpPost]
    public async Task<IActionResult> Create(int eventId, int sectorId, [FromBody] CreateSeatCommand command, CancellationToken ct)
    {
        command.EventId = eventId;
        command.SectorId = sectorId;
        var seatId = await _createSeatHandler.Handle(command, ct);

        return CreatedAtAction(nameof(GetSeatById), new { eventId, sectorId, id = seatId }, new SeatDto(
            seatId,
            command.SeatNumber,
            command.SectorId,
            string.Empty,
            command.Price,
            "Available"
        ));
    }

    [HttpGet]
    public async Task<IActionResult> GetSeats(int eventId, int sectorId, [FromQuery] GetSeatsQuery query, CancellationToken ct)
    {
        query.EventId = eventId;
        query.SectorId = sectorId;

        var seats = await _getSeatsHandler.Handle(query, ct);
        return Ok(seats);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetSeatById(int eventId, int sectorId, int id, CancellationToken ct)
    {
        var seatDto = await _getSeatByIdHandler.Handle(new GetSeatByIdQuery
        {
            EventId = eventId,
            SectorId = sectorId,
            SeatId = id
        }, ct);

        if (seatDto is null)
            return NotFound();

        return Ok(seatDto);
    }
}
