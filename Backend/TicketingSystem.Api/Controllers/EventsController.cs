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
    private readonly ICommandHandler<UpdateEventCommand> _updateEventHandler;
    private readonly ICommandHandler<DeleteEventCommand> _deleteEventHandler;
    private readonly IQueryHandler<GetEventsQuery, IEnumerable<EventDto>> _getEventsHandler;

    public EventsController(
        ICommandHandler<CreateEventCommand, int> createEventHandler,
        ICommandHandler<UpdateEventCommand> updateEventHandler,
        ICommandHandler<DeleteEventCommand> deleteEventHandler,
        IQueryHandler<GetEventsQuery, IEnumerable<EventDto>> getEventsHandler)
    {
        _createEventHandler = createEventHandler;
        _updateEventHandler = updateEventHandler;
        _deleteEventHandler = deleteEventHandler;
        _getEventsHandler = getEventsHandler;
    }

    /// <summary>
    /// List all events
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<EventDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEvents([FromQuery] GetEventsQuery query, CancellationToken cancellationToken)
    {
        var events = await _getEventsHandler.Handle(query, cancellationToken);
        return Ok(events);
    }

    /// <summary>
    /// Create a new event
    /// </summary>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateEventCommand command, CancellationToken cancellationToken)
    {
        var eventId = await _createEventHandler.Handle(command, cancellationToken);
        return CreatedAtAction(nameof(GetEvents), new { id = eventId }, new { Id = eventId });
    }

    /// <summary>
    /// Update an existing event
    /// </summary>
    [HttpPut("{id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateEventCommand command, CancellationToken cancellationToken)
    {
        command.Id = id;
        await _updateEventHandler.Handle(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Delete an event
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _deleteEventHandler.Handle(new DeleteEventCommand { Id = id }, cancellationToken);
        return NoContent();
    }
}
