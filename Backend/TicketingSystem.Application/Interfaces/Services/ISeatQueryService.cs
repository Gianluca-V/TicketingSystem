using TicketingSystem.Application.DTOs;

namespace TicketingSystem.Application.Interfaces.Services;

public interface ISeatQueryService
{
    Task<IEnumerable<SeatDto>> GetSeatsBySectorAsync(int eventId, int sectorId, CancellationToken cancellationToken = default);
}
