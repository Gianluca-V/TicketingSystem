using TicketingSystem.Application.Interfaces.persistence;
using TicketingSystem.Application.Interfaces.Services;
using TicketingSystem.Domain.Entities;

namespace TicketingSystem.Application.UseCases.Reservation.DeleteReservation;

public class DeleteReservationHandler : ICommandHandler<DeleteReservationCommand>
{
    private readonly IReservationRepository _reservationRepository;
    private readonly ISeatRepository _seatRepository;
    private readonly IAuditRepository _auditRepository;
    private readonly IUnitOfWork _uow;
    private readonly ICacheService _cacheService;

    public DeleteReservationHandler(
        IReservationRepository reservationRepository,
        ISeatRepository seatRepository,
        IAuditRepository auditRepository,
        IUnitOfWork uow,
        ICacheService cacheService)
    {
        _reservationRepository = reservationRepository;
        _seatRepository = seatRepository;
        _auditRepository = auditRepository;
        _uow = uow;
        _cacheService = cacheService;
    }

    public async Task Handle(DeleteReservationCommand command, CancellationToken ct)
    {
        await _uow.BeginTransactionAsync(ct);
        try
        {
            var reservation = await _reservationRepository.GetByIdAsync(command.ReservationId, ct);
            if (reservation == null) return;

            // Release the seat
            var seat = await _seatRepository.GetByIdAsync(reservation.SeatId, ct);
            if (seat != null)
            {
                seat.Status = SeatStatus.Available;
                await _seatRepository.UpdateAsync(seat, ct);

                await _auditRepository.AddAsync(new AuditLog
                {
                    Action = AuditAction.Released,
                    ResourceType = "Seat",
                    ResourceId = seat.Id.ToString(),
                    Details = $"Seat released by user (reservation {reservation.Id} deleted)",
                    UserId = reservation.UserId
                }, ct);
            }

            await _reservationRepository.DeleteAsync(reservation, ct);
            await _uow.CommitTransactionAsync(ct);
            await _cacheService.RemoveByPrefixAsync("Reservations:List", ct);
        }
        catch
        {
            await _uow.RollbackTransactionAsync(ct);
            throw;
        }
    }
}
