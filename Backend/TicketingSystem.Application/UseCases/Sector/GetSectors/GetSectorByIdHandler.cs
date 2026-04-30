using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicketingSystem.Application.DTOs;
using TicketingSystem.Application.Interfaces.persistence;
using TicketingSystem.Application.Interfaces.Services;

namespace TicketingSystem.Application.UseCases.Sector.GetSectors
{
    public class GetSectorByIdHandler : IQueryHandler<GetSectorByIdQuery, SectorDto?>
    {
        private readonly ISectorRepository _sectorRepository;
        private readonly ICacheService _cacheService;

        public GetSectorByIdHandler(ISectorRepository sectorRepository, ICacheService cacheService)
        {
            _sectorRepository = sectorRepository;
            _cacheService = cacheService;
        }

        public async Task<SectorDto?> Handle(GetSectorByIdQuery query, CancellationToken ct)
        {
            var cacheKey = $"Sector:{query.SectorId}";
            var cachedSector = await _cacheService.GetAsync<SectorDto>(cacheKey, ct);
            if (cachedSector != null)
            {
                return cachedSector;
            }

            var sector = await _sectorRepository.GetByIdAsync(query.SectorId, ct);

            if (sector is null || sector.EventId != query.EventId)
                return null;

            var result = new SectorDto(
                sector.Id,
                sector.EventId,
                sector.Event?.Name ?? "Unknown",
                sector.Name,
                sector.Price,
                sector.Capacity
            );

            await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(10), ct);

            return result;
        }
    }
}
