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
    private readonly IQueryHandler<GetSeatsQuery, IEnumerable<SeatDto>> _getSeatsHandler;

    public SeatsController(
        IQueryHandler<GetSeatsQuery, IEnumerable<SeatDto>> getSeatsHandler)
    {
        _getSeatsHandler = getSeatsHandler;
    }

    [HttpGet]
    public async Task<IActionResult> GetSeats(int eventId, int sectorId, [FromQuery] GetSeatsQuery query, CancellationToken ct)
    {
        query.EventId = eventId;
        query.SectorId = sectorId;

        var seats = await _getSeatsHandler.Handle(query, ct);
        return Ok(seats);
    }
}
