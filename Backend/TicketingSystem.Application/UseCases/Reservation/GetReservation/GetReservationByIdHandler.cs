using TicketingSystem.Application.DTOs;
using TicketingSystem.Application.Interfaces.persistence;
using TicketingSystem.Application.Interfaces.Services;

namespace TicketingSystem.Application.UseCases.Reservation.GetReservation;

public class GetReservationByIdHandler : IQueryHandler<GetReservationByIdQuery, ReservationDto?>
{
    private readonly IReservationRepository _reservationRepository;
    private readonly ICacheService _cacheService;

    public GetReservationByIdHandler(IReservationRepository reservationRepository, ICacheService cacheService)
    {
        _reservationRepository = reservationRepository;
        _cacheService = cacheService;
    }

    public async Task<ReservationDto?> Handle(GetReservationByIdQuery query, CancellationToken ct)
    {
        var cacheKey = $"Reservation:{query.ReservationId}";
        var cachedReservation = await _cacheService.GetAsync<ReservationDto>(cacheKey, ct);
        if (cachedReservation != null)
        {
            return cachedReservation;
        }

        var reservation = await _reservationRepository.GetByIdAsync(query.ReservationId, ct);
        if (reservation == null) return null;

        var result = new ReservationDto(
            reservation.Id,
            reservation.SeatId,
            reservation.UserId.ToString(),
            reservation.ReservedAt,
            reservation.ExpiresAt,
            reservation.IsExpired
        );

        await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5), ct);

        return result;
    }
}
