using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TicketingSystem.Application.Interfaces.persistence;
using TicketingSystem.Domain.Entities;
using TicketingSystem.Infrastructure.Data;

namespace TicketingSystem.Infrastructure.Services;

public class ExpiredReservationWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ExpiredReservationWorker> _logger;

    public ExpiredReservationWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<ExpiredReservationWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Expired Reservation Worker started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var auditRepository = scope.ServiceProvider.GetRequiredService<IAuditRepository>();
                var reservationRepository = scope.ServiceProvider.GetRequiredService<IReservationRepository>();
                var seatRepository = scope.ServiceProvider.GetRequiredService<ISeatRepository>();

                // Get expired reservations
                var expiredReservations = await reservationRepository.GetExpiredAsync(stoppingToken);

                foreach (var reservation in expiredReservations)
                {
                    // Release seat
                    var seat = await seatRepository.GetByIdAsync(reservation.SeatId, stoppingToken);
                    if (seat != null && seat.Status == SeatStatus.Reserved)
                    {
                        seat.Status = SeatStatus.Available;
                        await seatRepository.UpdateAsync(seat, stoppingToken);

                        // Log release
                        await auditRepository.AddAsync(new AuditLog
                        {
                            UserId = "SYSTEM",
                            Action = AuditAction.Released,
                            ResourceType = "Seat",
                            ResourceId = reservation.SeatId,
                            Details = $"Seat {reservation.SeatId} released due to expired reservation {reservation.Id}"
                        }, stoppingToken);

                        // Delete reservation
                        await reservationRepository.DeleteAsync(reservation, stoppingToken);

                        await context.SaveChangesAsync(stoppingToken);

                        _logger.LogInformation("Released expired reservation {ReservationId} for seat {SeatId}", 
                            reservation.Id, reservation.SeatId);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing expired reservations");
            }

            // Check every 30 seconds
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }

        _logger.LogInformation("Expired Reservation Worker stopped");
    }
}
