using Microsoft.EntityFrameworkCore;
using TicketingSystem.Application.Interfaces.persistence;
using TicketingSystem.Domain.Entities;
using TicketingSystem.Infrastructure.Data;

namespace TicketingSystem.Infrastructure.Repositories;

public class ReservationRepository : IReservationRepository
{
    private readonly ApplicationDbContext _context;

    public ReservationRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Reservation?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Reservations
            .Include(r => r.Seat)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<Reservation?> GetActiveAsync(int seatId, CancellationToken cancellationToken = default)
    {
        return await _context.Reservations
            .Where(r => r.SeatId == seatId && !r.PaidAt.HasValue && r.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(r => r.ReservedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task AddAsync(Reservation reservation, CancellationToken cancellationToken = default)
    {
        await _context.Reservations.AddAsync(reservation, cancellationToken);
    }

    public async Task UpdateAsync(Reservation reservation, CancellationToken cancellationToken = default)
    {
        _context.Reservations.Update(reservation);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(Reservation reservation, CancellationToken cancellationToken = default)
    {
        _context.Reservations.Remove(reservation);
        await Task.CompletedTask;
    }

    public async Task<IEnumerable<Reservation>> GetExpiredAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Reservations
            .Where(r => !r.PaidAt.HasValue && r.ExpiresAt < DateTime.UtcNow)
            .ToListAsync(cancellationToken);
    }
}
