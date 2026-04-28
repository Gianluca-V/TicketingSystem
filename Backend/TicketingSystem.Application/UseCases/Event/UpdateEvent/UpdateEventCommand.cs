using TicketingSystem.Application.Interfaces.Services;

namespace TicketingSystem.Application.UseCases.Event.UpdateEvent;

public class UpdateEventCommand : ICommand
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public DateTime? Date { get; set; }
    public string? Venue { get; set; }
    public string? Status { get; set; }
}
