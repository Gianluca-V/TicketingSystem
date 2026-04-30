using Microsoft.EntityFrameworkCore;
using TicketingSystem.Application.Interfaces.persistence;
using TicketingSystem.Domain.Entities;
using TicketingSystem.Domain.QueryFilters;
using TicketingSystem.Infrastructure.Data;

namespace TicketingSystem.Infrastructure.Repositories;

public class SeatRepository : ISeatRepository
{
    private readonly ApplicationDbContext _context;

    public SeatRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Seat>> GetAllAsync(SeatFilter filter, CancellationToken cancellationToken)
    {
        IQueryable<Seat> query = _context.Seats;

        if (!string.IsNullOrEmpty(filter.Status))
            query = query.Where(s => s.Status.ToString() == filter.Status);

        if (filter.SectorId.HasValue)
            query = query.Where(s => s.SectorId == filter.SectorId);

        if (filter.EventId.HasValue)
        {
            var sectorIds = _context.Sectors
                .Where(sec => sec.EventId == filter.EventId)
                .Select(sec => sec.Id);

            query = query.Where(s => sectorIds.Contains(s.SectorId));
        }

        query = query.OrderBy(s => s.SeatNumber)
                     .ApplyPaging(filter.Page, filter.Take);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<Seat?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Seats.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task AddAsync(Seat seat, CancellationToken cancellationToken = default)
    {
        await _context.Seats.AddAsync(seat, cancellationToken);
    }

    public async Task AddBulkAsync(IEnumerable<Seat> seats, CancellationToken cancellationToken = default)
    {
        await _context.Seats.AddRangeAsync(seats, cancellationToken);
    }

    public async Task UpdateAsync(Seat seat, CancellationToken ct)
    {
        _context.Seats.Update(seat);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var seat = await GetByIdAsync(id, ct);
        if (seat != null)
        {
            _context.Seats.Remove(seat);
        }
    }
}
