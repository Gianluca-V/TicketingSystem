using TicketingSystem.Application.DTOs;

namespace TicketingSystem.Application.Services;

public interface IReservationCommandService
{
    Task<ReserveSeatResponse> ReserveSeatAsync(int eventId, int sectorId, string userId, int seatId, CancellationToken cancellationToken = default);
}
