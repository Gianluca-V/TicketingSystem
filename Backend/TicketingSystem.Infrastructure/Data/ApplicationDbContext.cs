using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using TicketingSystem.Domain.Entities;
using TicketingSystem.Domain.Interfaces;

namespace TicketingSystem.Infrastructure.Data;

public class ApplicationDbContext : DbContext, IUnitOfWork
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

        // Configure RowVersion for optimistic locking
        // PostgreSQL doesn't have native RowVersion, so we use a regular byte array
        // and rely on EF Core's concurrency token mechanism
        modelBuilder.Entity<Seat>()
            .Property(s => s.RowVersion)
            .HasColumnName("RowVersion")
            .HasColumnType("bytea")
            .IsConcurrencyToken();

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
        modelBuilder.Entity<Event>()
            .HasMany(e => e.Sectors)
            .WithOne(s => s.Event)
            .HasForeignKey(s => s.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Sector>()
            .HasMany(s => s.Seats)
            .WithOne(s => s.Sector)
            .HasForeignKey(s => s.SectorId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Seat>()
            .HasMany(s => s.Reservations)
            .WithOne(r => r.Seat)
            .HasForeignKey(r => r.SeatId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Use NoTracking by default for read operations
        optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
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
        try
        {
            await SaveChangesAsync(cancellationToken);
            await _currentTransaction?.CommitAsync(cancellationToken)!;
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            _currentTransaction?.Dispose();
            _currentTransaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _currentTransaction?.RollbackAsync(cancellationToken)!;
        }
        finally
        {
            _currentTransaction?.Dispose();
            _currentTransaction = null;
        }
    }
}
