using TicketingSystem.Application.Interfaces.Services;

namespace TicketingSystem.Application.UseCases.Event.CreateEvent;

public class CreateEventCommand : ICommand
{
    public required string Name { get; set; }
    public DateTime Date { get; set; }
    public string Venue { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";
}
