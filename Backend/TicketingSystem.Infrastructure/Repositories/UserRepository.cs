using Microsoft.EntityFrameworkCore;
using TicketingSystem.Application.Interfaces.persistence;
using TicketingSystem.Domain.Entities;
using TicketingSystem.Domain.QueryFilters;
using TicketingSystem.Infrastructure.Data;

namespace TicketingSystem.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Id == id, ct);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email, ct);
    }

    public async Task<IEnumerable<User>> GetAllAsync(UserFilter filter, CancellationToken ct = default)
    {
        IQueryable<User> query = _context.Users;

        if (!string.IsNullOrEmpty(filter.Name))
            query = query.Where(u => u.Name.Contains(filter.Name));

        if (!string.IsNullOrEmpty(filter.Email))
            query = query.Where(u => u.Email.Contains(filter.Email));

        query = query.ApplyPaging(filter.Page, filter.Take);

        return await query.ToListAsync(ct);
    }

    public async Task AddAsync(User user, CancellationToken ct = default)
    {
        await _context.Users.AddAsync(user, ct);
    }

    public async Task UpdateAsync(User user, CancellationToken ct = default)
    {
        _context.Users.Update(user);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var user = await GetByIdAsync(id, ct);
        if (user != null)
        {
            _context.Users.Remove(user);
        }
    }
}
