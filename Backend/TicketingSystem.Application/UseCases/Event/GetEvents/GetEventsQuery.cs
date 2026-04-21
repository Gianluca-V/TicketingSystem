using TicketingSystem.Application.DTOs;
using TicketingSystem.Application.Interfaces.Services;

namespace TicketingSystem.Application.UseCases.Event.GetEvents;

public class GetEventsQuery : IQuery<IEnumerable<EventDto>>
{
    public string? Name { get; set; }
    public string? Status { get; set; }
    public string? Venue { get; set; }
    public int Page { get; set; } = 1;
    public int Take { get; set; } = 100;
}
