using Microsoft.AspNetCore.Mvc;
using TicketingSystem.Application.Interfaces.Services;

namespace TicketingSystem.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class EventsController : ControllerBase
{
    //private readonly IEventQueryService _eventQueryService;
    //private readonly ISeatQueryService _seatQueryService;
    private readonly ILogger<EventsController> _logger;

    public EventsController(
        IEventQueryService eventQueryService,
        ISeatQueryService seatQueryService,
        ILogger<EventsController> logger)
    {
        //_eventQueryService = eventQueryService;
        //_seatQueryService = seatQueryService;
        _logger = logger;
    }

    /// <summary>
    /// List all events
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Application.DTOs.EventDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEvents(CancellationToken cancellationToken)
    {
        //var events = await _eventQueryService.GetEventsAsync(cancellationToken);
        //return Ok(events);
        return Ok();
    }

    /// <summary>
    /// Get seats for a specific sector in an event
    /// </summary>
    [HttpGet("{eventId}/sectors/{sectorId}/seats")]
    [ProducesResponseType(typeof(IEnumerable<Application.DTOs.SeatDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSeats(int eventId, int sectorId, CancellationToken cancellationToken)
    {
       /* var seats = await _seatQueryService.GetSeatsBySectorAsync(eventId, sectorId, cancellationToken);
        return Ok(seats);*/
        return Ok();
    }
}
