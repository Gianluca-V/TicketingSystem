using TicketingSystem.Application.Interfaces.Services;

namespace TicketingSystem.Application.UseCases.Sector.DeleteSector;

public class DeleteSectorCommand : ICommand
{
    public int Id { get; set; }
}
