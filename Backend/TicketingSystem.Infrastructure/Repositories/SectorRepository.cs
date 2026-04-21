using Microsoft.EntityFrameworkCore;
using TicketingSystem.Application.Interfaces.persistence;
using TicketingSystem.Domain.Entities;
using TicketingSystem.Domain.QueryFilters;
using TicketingSystem.Infrastructure.Data;

namespace TicketingSystem.Infrastructure.Repositories;

public class SectorRepository : ISectorRepository
{
    private readonly ApplicationDbContext _context;

    public SectorRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Sector?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _context.Sectors.Include(s => s.Event).FirstOrDefaultAsync(s => s.Id == id, ct);
    }

    public async Task<IEnumerable<Sector>> GetAllAsync(SectorFilter filter, CancellationToken ct = default)
    {
        IQueryable<Sector> query = _context.Sectors.Include(s => s.Event);

        if (filter.EventId.HasValue)
            query = query.Where(s => s.EventId == filter.EventId);

        if (!string.IsNullOrEmpty(filter.Name))
            query = query.Where(s => s.Name.Contains(filter.Name));

        query = query.ApplyPaging(filter.Page, filter.Take);

        return await query.ToListAsync(ct);
    }

    public async Task AddAsync(Sector sector, CancellationToken ct = default)
    {
        await _context.Sectors.AddAsync(sector, ct);
    }

    public async Task UpdateAsync(Sector sector, CancellationToken ct = default)
    {
        _context.Sectors.Update(sector);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var sector = await GetByIdAsync(id, ct);
        if (sector != null)
        {
            _context.Sectors.Remove(sector);
        }
    }
}
