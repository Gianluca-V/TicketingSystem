using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using TicketingSystem.Application.Interfaces.persistence;
using TicketingSystem.Domain.Entities;

namespace TicketingSystem.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<int>, int>, IUnitOfWork
{
    private IDbContextTransaction? _currentTransaction;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Event> Events => Set<Event>();
    public DbSet<Sector> Sectors => Set<Sector>();
    public DbSet<Seat> Seats => Set<Seat>();
    public DbSet<Reservation> Reservations => Set<Reservation>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>().ToTable("Users");

        modelBuilder.Entity<Seat>()
            .Property(e => e.Version)
            .HasColumnName("xmin")
            .IsRowVersion();

        modelBuilder.Entity<Reservation>()
            .Property(r => r.Id)
            .HasDefaultValueSql("gen_random_uuid()");

        modelBuilder.Entity<AuditLog>()
            .Property(a => a.Id)
            .HasDefaultValueSql("gen_random_uuid()");

        // Unique constraint for sector + seat number
        modelBuilder.Entity<Seat>()
            .HasIndex(s => new { s.SectorId, s.SeatNumber })
            .IsUnique();

        // Partial index for active reservations
        modelBuilder.Entity<Reservation>()
            .HasIndex(r => new { r.SeatId, r.PaidAt })
            .HasFilter("\"PaidAt\" IS NULL");

        // Configure all DateTime columns as timestamp with time zone
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
                {
                    property.SetColumnType("timestamp with time zone");
                }
            }
        }

        // Configure relationships
        modelBuilder.Entity<Sector>()
            .HasOne<Event>()
            .WithMany()
            .HasForeignKey(s => s.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Seat>()
            .HasOne<Sector>()
            .WithMany()
            .HasForeignKey(s => s.SectorId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Reservation>()
            .HasOne<Seat>()
            .WithMany()
            .HasForeignKey(r => r.SeatId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
    }

    // IUnitOfWork implementation
    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction != null)
        {
            return;
        }

        _currentTransaction = await Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction == null) return;

        try
        {
            await SaveChangesAsync(cancellationToken);
            await _currentTransaction.CommitAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            var transaction = _currentTransaction;
            _currentTransaction = null;
            transaction?.Dispose();
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction == null) return;

        try
        {
            await _currentTransaction.RollbackAsync(cancellationToken);
        }
        finally
        {
            _currentTransaction.Dispose();
            _currentTransaction = null;
        }
    }
}
