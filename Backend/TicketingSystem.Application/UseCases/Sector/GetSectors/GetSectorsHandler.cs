using TicketingSystem.Application.DTOs;
using TicketingSystem.Application.Interfaces.persistence;
using TicketingSystem.Application.Interfaces.Services;
using TicketingSystem.Domain.QueryFilters;

namespace TicketingSystem.Application.UseCases.Sector.GetSectors;

public class GetSectorsHandler : IQueryHandler<GetSectorsQuery, IEnumerable<SectorDto>>
{
    private readonly ISectorRepository _sectorRepository;
    private readonly ICacheService _cacheService;

    public GetSectorsHandler(ISectorRepository sectorRepository, ICacheService cacheService)
    {
        _sectorRepository = sectorRepository;
        _cacheService = cacheService;
    }

    public async Task<IEnumerable<SectorDto>> Handle(GetSectorsQuery query, CancellationToken ct)
    {
        var cacheKey = $"Sectors:List:{query.EventId}:{query.Name}:{query.Page}:{query.Take}";
        var cachedSectors = await _cacheService.GetAsync<IEnumerable<SectorDto>>(cacheKey, ct);
        if (cachedSectors != null)
        {
            return cachedSectors;
        }

        var filter = new SectorFilter
        {
            EventId = query.EventId,
            Name = query.Name,
            Page = query.Page,
            Take = query.Take
        };

        var sectors = await _sectorRepository.GetAllAsync(filter, ct);

        var result = sectors.Select(s => new SectorDto(
            s.Id,
            s.EventId,
            s.Event?.Name ?? "Unknown",
            s.Name,
            s.Price,
            s.Capacity
        )).ToList();

        await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(10), ct);

        return result;
    }
}
