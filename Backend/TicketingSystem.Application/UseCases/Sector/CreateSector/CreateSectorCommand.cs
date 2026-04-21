using TicketingSystem.Application.Interfaces.Services;

namespace TicketingSystem.Application.UseCases.Sector.CreateSector;

public class CreateSectorCommand : ICommand
{
    public int EventId { get; set; }
    public required string Name { get; set; }
    public decimal Price { get; set; }
    public int Capacity { get; set; }
}
