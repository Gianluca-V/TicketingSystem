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

        public GetSectorByIdHandler(ISectorRepository sectorRepository)
        {
            _sectorRepository = sectorRepository;
        }

        public async Task<SectorDto?> Handle(GetSectorByIdQuery query, CancellationToken ct)
        {
            var sector = await _sectorRepository.GetByIdAsync(query.SectorId, ct);

            if (sector is null || sector.EventId != query.EventId)
                return null;

            return new SectorDto(
                sector.Id,
                sector.EventId,
                sector.Event?.Name ?? "Unknown",
                sector.Name,
                sector.Price,
                sector.Capacity
            );
        }
    }
}
