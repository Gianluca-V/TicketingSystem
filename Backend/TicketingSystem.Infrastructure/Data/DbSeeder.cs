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

        var concertEvent = new Event
        {
            Name      = "Rock Festival 2026",
            EventDate = new DateTime(2026, 8, 15, 20, 0, 0, DateTimeKind.Utc),
            Venue     = "Estadio Obras Sanitarias",
            Status    = "Active"
        };

        context.Events.Add(concertEvent);
        await context.SaveChangesAsync();

        // Layout del mapa (igual a la captura de referencia):
        //   VIP               → tira PIT debajo del escenario
        //   Campo Delantero   → mitad superior del área central
        //   Campo General     → mitad inferior del área central
        //   Platea 1 Izq/Der  → fila superior de los costados
        //   Platea 2 Izq/Der  → fila inferior de los costados
        var sectors = new[]
        {
            new Sector { Name = "VIP",               EventId = concertEvent.Id, Event = concertEvent, Price = 200.00m, Capacity =  50 },
            new Sector { Name = "Campo",             EventId = concertEvent.Id, Event = concertEvent, Price =  60.00m, Capacity = 400 },
            new Sector { Name = "Platea Izquierda",  EventId = concertEvent.Id, Event = concertEvent, Price = 140.00m, Capacity = 200 },
            new Sector { Name = "Platea Izquierda 2",EventId = concertEvent.Id, Event = concertEvent, Price =  80.00m, Capacity = 100 },
            new Sector { Name = "Platea Derecha",    EventId = concertEvent.Id, Event = concertEvent, Price = 140.00m, Capacity = 200 },
            new Sector { Name = "Platea Derecha 2",  EventId = concertEvent.Id, Event = concertEvent, Price =  80.00m, Capacity = 100 },
        };

        context.Sectors.AddRange(sectors);
        await context.SaveChangesAsync();

        var seatDefs = new[]
        {
            (sectors[0], "VIP",   50,  200.00m),
            (sectors[1], "CAM",  400,   60.00m),
            (sectors[2], "PIZQ", 200,  140.00m),
            (sectors[3], "PIZ2", 100,   80.00m),
            (sectors[4], "PDER", 200,  140.00m),
            (sectors[5], "PDR2", 100,   80.00m),
        };

        foreach (var (sector, prefix, count, price) in seatDefs)
        {
            for (int n = 1; n <= count; n++)
            {
                context.Seats.Add(new Seat
                {
                    SectorId   = sector.Id,
                    SeatNumber = $"{prefix}-{n:D3}",
                    Price      = price,
                    Status     = SeatStatus.Available
                });
            }
        }

        await context.SaveChangesAsync();
    }

    public static async Task EnsureMigrationsAppliedAsync(ApplicationDbContext context)
    {
        await context.Database.EnsureCreatedAsync();
    }
}
