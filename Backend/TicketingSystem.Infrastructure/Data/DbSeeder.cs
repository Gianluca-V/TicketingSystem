using Microsoft.EntityFrameworkCore;
using TicketingSystem.Domain.Entities;
using TicketingSystem.Infrastructure.Data;

namespace TicketingSystem.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        try
        {
            if (await context.Events.AnyAsync())
            {
                return; // Already seeded
            }
        }
        catch
        {
            // If table doesn't exist, proceed with seeding
        }

        // Create event
        var concertEvent = new Event
        {
            Name = "Rock Festival 2026",
            Date = new DateTime(2026, 8, 15, 20, 0, 0, DateTimeKind.Utc),
            Sectors = new List<Sector>()
        };

        // Create sectors
        var vipSector = new Sector
        {
            Name = "VIP",
            Event = concertEvent,
            Seats = new List<Seat>()
        };

        var generalSector = new Sector
        {
            Name = "General",
            Event = concertEvent,
            Seats = new List<Seat>()
        };

        concertEvent.Sectors.Add(vipSector);
        concertEvent.Sectors.Add(generalSector);

        // Create 50 VIP seats
        for (int i = 1; i <= 50; i++)
        {
            vipSector.Seats.Add(new Seat
            {
                SeatNumber = $"VIP-{i:D3}",
                Price = 150.00m,
                Sector = vipSector
            });
        }

        // Create 50 General seats
        for (int i = 1; i <= 50; i++)
        {
            generalSector.Seats.Add(new Seat
            {
                SeatNumber = $"GEN-{i:D3}",
                Price = 75.00m,
                Sector = generalSector
            });
        }

        context.Events.Add(concertEvent);
        await context.SaveChangesAsync();
    }

    public static async Task EnsureMigrationsAppliedAsync(ApplicationDbContext context)
    {
        // Always use EnsureCreatedAsync for fresh database creation
        // This creates the schema from the model without needing migrations
        await context.Database.EnsureCreatedAsync();
    }
}
