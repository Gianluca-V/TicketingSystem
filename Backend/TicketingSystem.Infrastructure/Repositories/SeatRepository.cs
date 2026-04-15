using Microsoft.EntityFrameworkCore;
using TicketingSystem.Domain.Entities;
using TicketingSystem.Domain.Interfaces;
using TicketingSystem.Infrastructure.Data;

namespace TicketingSystem.Infrastructure.Repositories;

public class SeatRepository : ISeatRepository
{
    private readonly ApplicationDbContext _context;

    public SeatRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Seat?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Seats
            .Include(s => s.Sector)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task AddAsync(Seat seat, CancellationToken cancellationToken = default)
    {
        await _context.Seats.AddAsync(seat, cancellationToken);
    }

    public async Task UpdateAsync(Seat seat, CancellationToken cancellationToken = default)
    {
        _context.Seats.Update(seat);
        await Task.CompletedTask;
    }
}
