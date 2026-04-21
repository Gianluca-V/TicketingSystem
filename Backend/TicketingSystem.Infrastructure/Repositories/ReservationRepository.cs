using Microsoft.EntityFrameworkCore;
using TicketingSystem.Application.Interfaces.persistence;
using TicketingSystem.Domain.Entities;
using TicketingSystem.Domain.QueryFilters;
using TicketingSystem.Infrastructure.Data;

namespace TicketingSystem.Infrastructure.Repositories;

public class ReservationRepository : IReservationRepository
{
    private readonly ApplicationDbContext _context;

    public ReservationRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Reservation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Reservations.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Reservation>> GetAllAsync(ReservationFilter filter, CancellationToken cancellationToken)
    {
        IQueryable<Reservation> query = _context.Reservations;

        if (filter.SeatId.HasValue)
            query = query.Where(r => r.SeatId == filter.SeatId);

        if (!string.IsNullOrEmpty(filter.UserId))
        {
             if(int.TryParse(filter.UserId, out int userId))
                query = query.Where(r => r.UserId == userId);
        }

        if (filter.IsActive.HasValue)
        {
            if (filter.IsActive.Value)
                query = query.Where(r => !r.IsExpired);
            else
                query = query.Where(r => r.IsExpired);
        }

        query = query.ApplyPaging(filter.Page, filter.Take);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Reservation reservation, CancellationToken cancellationToken = default)
    {
        await _context.Reservations.AddAsync(reservation, cancellationToken);
    }

    public async Task UpdateAsync(Reservation reservation, CancellationToken cancellationToken = default)
    {
        _context.Reservations.Update(reservation);
    }

    public async Task DeleteAsync(Reservation reservation, CancellationToken cancellationToken = default)
    {
        _context.Reservations.Remove(reservation);
    }

    public async Task<IEnumerable<Reservation>> GetExpiredAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Reservations
            .Where(r => !r.PaidAt.HasValue && r.ExpiresAt < DateTime.UtcNow)
            .ToListAsync(cancellationToken);
    }
}
