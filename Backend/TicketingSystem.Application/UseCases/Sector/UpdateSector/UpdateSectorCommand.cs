using TicketingSystem.Application.Interfaces.Services;

namespace TicketingSystem.Application.UseCases.Sector.UpdateSector;

public class UpdateSectorCommand : ICommand
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public decimal? Price { get; set; }
    public int? Capacity { get; set; }
}
