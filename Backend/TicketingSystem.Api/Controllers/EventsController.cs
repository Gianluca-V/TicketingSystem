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
    private readonly ICommandHandler<CreateEventCommand, int> _createEventHandler;
    private readonly ICommandHandler<DeleteEventCommand> _deleteEventHandler;
    private readonly ICommandHandler<UpdateEventCommand> _updateEventHandler;
    private readonly IQueryHandler<GetEventsQuery, IEnumerable<EventDto>> _getEventsHandler;
    private readonly IQueryHandler<GetEventByIdQuery, EventDto?> _getEventByIdHandler;

    public EventsController(
        ICommandHandler<CreateEventCommand, int> createEventHandler,
        ICommandHandler<DeleteEventCommand> deleteEventHandler,
        ICommandHandler<UpdateEventCommand> updateEventHandler,
        IQueryHandler<GetEventsQuery, IEnumerable<EventDto>> getEventsHandler,
        IQueryHandler<GetEventByIdQuery, EventDto?> getEventByIdHandler)
    {
        _createEventHandler = createEventHandler;
        _deleteEventHandler = deleteEventHandler;
        _updateEventHandler = updateEventHandler;
        _getEventsHandler = getEventsHandler;
        _getEventByIdHandler = getEventByIdHandler;
    }

    [HttpGet]
    public async Task<IActionResult> GetEvents([FromQuery] GetEventsQuery query, CancellationToken ct)
    {
        var events = await _getEventsHandler.Handle(query, ct);
        return Ok(events);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateEventCommand command, CancellationToken ct)
    {
        var eventId = await _createEventHandler.Handle(command, ct);
        return CreatedAtAction(nameof(GetEventById), new { id = eventId }, new { Id = eventId });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetEventById(int id, CancellationToken ct)
    {
        var eventDto = await _getEventByIdHandler.Handle(new GetEventByIdQuery { Id = id }, ct);

        if (eventDto is null)
            return NotFound();

        return Ok(eventDto);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateEventCommand command, CancellationToken ct)
    {
        command.Id = id;
        await _updateEventHandler.Handle(command, ct);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _deleteEventHandler.Handle(new DeleteEventCommand { Id = id }, ct);
        return NoContent();
    }
}
