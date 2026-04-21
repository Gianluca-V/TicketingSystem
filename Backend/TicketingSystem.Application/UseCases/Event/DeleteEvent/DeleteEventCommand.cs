using TicketingSystem.Application.Interfaces.Services;

namespace TicketingSystem.Application.UseCases.Event.DeleteEvent;

public class DeleteEventCommand : ICommand
{
    public int Id { get; set; }
}
