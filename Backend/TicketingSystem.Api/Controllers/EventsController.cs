using Microsoft.AspNetCore.Mvc;
using TicketingSystem.Application.DTOs;
using TicketingSystem.Application.Interfaces.Services;
using TicketingSystem.Application.UseCases.Event.GetEvents;
using TicketingSystem.Application.UseCases.Seat.GetSeats;

namespace TicketingSystem.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class EventsController : ControllerBase
{
    private readonly IQueryHandler<GetEventsQuery, IEnumerable<EventDto>> _getEventsHandler;
    private readonly IQueryHandler<GetSeatsQuery, IEnumerable<SeatDto>> _getSeatsHandler;
    private readonly ILogger<EventsController> _logger;

    public EventsController(
        IQueryHandler<GetEventsQuery, IEnumerable<EventDto>> getEventsHandler,
        IQueryHandler<GetSeatsQuery, IEnumerable<SeatDto>> getSeatsHandler,
        ILogger<EventsController> logger)
    {
        _getEventsHandler = getEventsHandler;
        _getSeatsHandler = getSeatsHandler;
        _logger = logger;
    }

    /// <summary>
    /// List all events
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Application.DTOs.EventDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEvents(CancellationToken cancellationToken)
    {
        var events = await _getEventsHandler.Handle(new GetEventsQuery(), cancellationToken);
        return Ok(events);
    }

    /// <summary>
    /// Get seats for a specific sector in an event
    /// </summary>
    [HttpGet("{eventId}/sectors/{sectorId}/seats")]
    [ProducesResponseType(typeof(IEnumerable<Application.DTOs.SeatDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSeats(int eventId, int sectorId, CancellationToken cancellationToken)
    {
        var seats = await _getSeatsHandler.Handle(new GetSeatsQuery { EventId = eventId, SectorId = sectorId }, cancellationToken);
        return Ok(seats);
    }
}
