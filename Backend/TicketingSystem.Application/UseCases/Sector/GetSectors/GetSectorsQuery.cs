using TicketingSystem.Application.DTOs;
using TicketingSystem.Application.Interfaces.Services;

namespace TicketingSystem.Application.UseCases.Sector.GetSectors;

public class GetSectorsQuery : IQuery<IEnumerable<SectorDto>>
{
    public int? EventId { get; set; }
    public string? Name { get; set; }
    public int Page { get; set; } = 1;
    public int Take { get; set; } = 100;
}
