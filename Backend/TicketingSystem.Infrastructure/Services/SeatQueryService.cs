using Microsoft.EntityFrameworkCore;
using TicketingSystem.Application.DTOs;
using TicketingSystem.Application.Services;
using TicketingSystem.Domain.Exceptions;
using TicketingSystem.Infrastructure.Data;

namespace TicketingSystem.Infrastructure.Services;

public class SeatQueryService : ISeatQueryService
{
    private readonly ApplicationDbContext _context;

    public SeatQueryService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<SeatDto>> GetSeatsBySectorAsync(int eventId, int sectorId, CancellationToken cancellationToken = default)
    {
        var sectorExists = await _context.Sectors
            .AnyAsync(s => s.Id == sectorId && s.EventId == eventId, cancellationToken);

        if (!sectorExists)
        {
            throw new KeyNotFoundException($"Sector {sectorId} not found in event {eventId}");
        }

        return await _context.Seats
            .Where(s => s.SectorId == sectorId)
            .Select(s => new SeatDto(
                s.Id,
                s.SeatNumber,
                s.Sector.Name,
                s.Price,
                s.Status.ToString()))
            .ToListAsync(cancellationToken);
    }
}
