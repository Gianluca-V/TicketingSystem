using TicketingSystem.Application.DTOs;
using TicketingSystem.Application.Interfaces.persistence;
using TicketingSystem.Application.Interfaces.Services;
using TicketingSystem.Domain.QueryFilters;

namespace TicketingSystem.Application.UseCases.Sector.GetSectors;

public class GetSectorsHandler : IQueryHandler<GetSectorsQuery, IEnumerable<SectorDto>>
{
    private readonly ISectorRepository _sectorRepository;

    public GetSectorsHandler(ISectorRepository sectorRepository)
    {
        _sectorRepository = sectorRepository;
    }

    public async Task<IEnumerable<SectorDto>> Handle(GetSectorsQuery query, CancellationToken ct)
    {
        var filter = new SectorFilter
        {
            EventId = query.EventId,
            Name = query.Name,
            Page = query.Page,
            Take = query.Take
        };

        var sectors = await _sectorRepository.GetAllAsync(filter, ct);

        return sectors.Select(s => new SectorDto(
            s.Id,
            s.EventId,
            s.Event?.Name ?? "Unknown",
            s.Name,
            s.Price,
            s.Capacity
        ));
    }
}
