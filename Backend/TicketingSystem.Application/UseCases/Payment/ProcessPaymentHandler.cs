using TicketingSystem.Application.DTOs;
using TicketingSystem.Application.Interfaces.persistence;
using TicketingSystem.Application.Interfaces.Services;
using TicketingSystem.Domain.Entities;
using TicketingSystem.Domain.Exceptions;

namespace TicketingSystem.Application.UseCases.Payment;

public class ProcessPaymentHandler : ICommandHandler<ProcessPaymentCommand, PaymentResponse>
{
    private readonly ISeatRepository _seatRepository;
    private readonly IReservationRepository _reservationRepository;
    private readonly IAuditRepository _auditRepository;
    private readonly ICacheService _cacheService;
    private readonly IUnitOfWork _uow;

    public ProcessPaymentHandler(
        ISeatRepository seatRepository,
        IReservationRepository reservationRepository,
        IAuditRepository auditRepository,
        ICacheService cacheService,
        IUnitOfWork uow)
    {
        _seatRepository = seatRepository;
        _reservationRepository = reservationRepository;
        _auditRepository = auditRepository;
        _cacheService = cacheService;
        _uow = uow;
    }

    public async Task<PaymentResponse> Handle(ProcessPaymentCommand command, CancellationToken ct)
    {
        var reservation = await _reservationRepository.GetByIdAsync(command.ReservationId, ct);
        if (reservation == null)
        {
            throw new KeyNotFoundException($"Reservation {command.ReservationId} not found");
        }

        if (reservation.PaidAt.HasValue)
        {
            throw new ConflictException($"Reservation {command.ReservationId} is already paid");
        }

        if (reservation.IsExpired)
        {
            throw new ConflictException($"Reservation {command.ReservationId} has expired");
        }

        await _uow.BeginTransactionAsync(ct);
        try
        {
            var seat = await _seatRepository.GetByIdAsync(reservation.SeatId, ct);
            if (seat == null)
            {
                throw new KeyNotFoundException($"Seat {reservation.SeatId} not found");
            }

            seat.Status = SeatStatus.Sold;
            reservation.PaidAt = DateTime.UtcNow;

            await _seatRepository.UpdateAsync(seat, ct);
            await _reservationRepository.UpdateAsync(reservation, ct);

            await _auditRepository.AddAsync(new AuditLog
            {
                UserId = reservation.UserId,
                Action = AuditAction.PaymentConfirmed,
                ResourceType = "Seat",
                ResourceId = reservation.SeatId.ToString(),
                Details = $"Payment confirmed. Transaction: {command.TransactionId}. Seat sold to user {reservation.UserId}"
            }, ct);

            await _uow.CommitTransactionAsync(ct);

            // Invalidate cache
            await _cacheService.RemoveByPrefixAsync("Reservations:List", ct);
            await _cacheService.RemoveByPrefixAsync("AuditLogs:List", ct);
            await _cacheService.RemoveByPrefixAsync("Seats:List", ct);

            return new PaymentResponse("Sold");
        }
        catch
        {
            await _uow.RollbackTransactionAsync(ct);
            throw;
        }
    }
}
