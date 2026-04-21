using TicketingSystem.Application.DTOs;
using TicketingSystem.Application.Interfaces.persistence;
using TicketingSystem.Application.Interfaces.Services;

namespace TicketingSystem.Application.UseCases.Reservation.GetReservation;

public class GetReservationHandler : IQueryHandler<GetReservationQuery, ReservationDto?>
{
    private readonly IReservationRepository _reservationRepository;

    public GetReservationHandler(IReservationRepository reservationRepository)
    {
        _reservationRepository = reservationRepository;
    }

    public async Task<ReservationDto?> Handle(GetReservationQuery query, CancellationToken ct)
    {
        var reservation = await _reservationRepository.GetByIdAsync(query.ReservationId, ct);
        if (reservation == null) return null;

        return new ReservationDto(
            reservation.Id,
            reservation.SeatId,
            reservation.UserId.ToString(),
            reservation.ReservedAt,
            reservation.ExpiresAt,
            reservation.IsExpired
        );
    }
}
