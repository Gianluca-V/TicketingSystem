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
            EventDate = new DateTime(2026, 8, 15, 20, 0, 0, DateTimeKind.Utc),
            Venue = "Wembley Stadium",
            Status = "Active"
        };

        context.Events.Add(concertEvent);
        await context.SaveChangesAsync();

        // Create sectors
        var vipSector = new Sector
        {
            Name = "VIP",
            EventId = concertEvent.Id,
            Event = concertEvent, // EF needs the navigation for validation if required
            Price = 150.00m,
            Capacity = 50
        };

        var generalSector = new Sector
        {
            Name = "General",
            EventId = concertEvent.Id,
            Event = concertEvent,
            Price = 75.00m,
            Capacity = 50
        };

        context.Sectors.Add(vipSector);
        context.Sectors.Add(generalSector);
        await context.SaveChangesAsync();

        // Create 50 VIP seats
        for (int i = 1; i <= 50; i++)
        {
            context.Seats.Add(new Seat
            {
                SectorId = vipSector.Id,
                SeatNumber = $"VIP-{i:D3}",
                Price = 150.00m,
                Status = SeatStatus.Available
            });
        }

        // Create 50 General seats
        for (int i = 1; i <= 50; i++)
        {
            context.Seats.Add(new Seat
            {
                SectorId = generalSector.Id,
                SeatNumber = $"GEN-{i:D3}",
                Price = 75.00m,
                Status = SeatStatus.Available
            });
        }

        await context.SaveChangesAsync();
    }

    public static async Task EnsureMigrationsAppliedAsync(ApplicationDbContext context)
    {
        // Always use EnsureCreatedAsync for fresh database creation
        // This creates the schema from the model without needing migrations
        await context.Database.EnsureCreatedAsync();
    }
}
