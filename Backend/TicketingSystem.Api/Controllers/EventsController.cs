using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TicketingSystem.Application.DTOs;
using TicketingSystem.Application.Interfaces.Services;
using TicketingSystem.Application.UseCases.Event.CreateEvent;
using TicketingSystem.Application.UseCases.Event.DeleteEvent;
using TicketingSystem.Application.UseCases.Event.GetEvents;
using TicketingSystem.Application.UseCases.Event.UpdateEvent;

namespace TicketingSystem.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class EventsController : ControllerBase
{
    private readonly IQueryHandler<GetEventsQuery, IEnumerable<EventDto>> _getEventsHandler;

    public EventsController(
        IQueryHandler<GetEventsQuery, IEnumerable<EventDto>> getEventsHandler)
    {
        _getEventsHandler = getEventsHandler;
    }

    [HttpGet]
    public async Task<IActionResult> GetEvents([FromQuery] GetEventsQuery query, CancellationToken ct)
    {
        var events = await _getEventsHandler.Handle(query, ct);
        return Ok(events);
    }
}
