using TicketingSystem.Application.DTOs;
using TicketingSystem.Application.Interfaces.persistence;
using TicketingSystem.Application.Interfaces.Services;
using TicketingSystem.Domain.Entities;
using TicketingSystem.Domain.Exceptions;

namespace TicketingSystem.Infrastructure.Services;

public class PaymentCommandService : IPaymentCommandService
{
    private readonly ISeatRepository _seatRepository;
    private readonly IReservationRepository _reservationRepository;
    private readonly IAuditRepository _auditRepository;
    private readonly IUnitOfWork _unitOfWork;

    public PaymentCommandService(
        ISeatRepository seatRepository,
        IReservationRepository reservationRepository,
        IAuditRepository auditRepository,
        IUnitOfWork unitOfWork)
    {
        _seatRepository = seatRepository;
        _reservationRepository = reservationRepository;
        _auditRepository = auditRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<PaymentResponse> ProcessPaymentAsync(int reservationId, string transactionId, CancellationToken cancellationToken = default)
    {
        var reservation = await _reservationRepository.GetByIdAsync(reservationId, cancellationToken);
        if (reservation == null)
        {
            throw new KeyNotFoundException($"Reservation {reservationId} not found");
        }

        if (reservation.PaidAt.HasValue)
        {
            throw new BusinessException($"Reservation {reservationId} is already paid");
        }

        if (reservation.IsExpired)
        {
            throw new BusinessException($"Reservation {reservationId} has expired");
        }

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var seat = await _seatRepository.GetByIdAsync(reservation.SeatId, cancellationToken);
            if (seat == null)
            {
                throw new KeyNotFoundException($"Seat {reservation.SeatId} not found");
            }

            seat.Status = SeatStatus.Sold;
            reservation.PaidAt = DateTime.UtcNow;

            await _seatRepository.UpdateAsync(seat, cancellationToken);
            await _reservationRepository.UpdateAsync(reservation, cancellationToken);

            await _auditRepository.AddAsync(new AuditLog
            {
                UserId = reservation.UserId,
                Action = AuditAction.PaymentConfirmed,
                ResourceType = "Seat",
                ResourceId = reservation.SeatId,
                Details = $"Payment confirmed. Transaction: {transactionId}. Seat sold to user {reservation.UserId}"
            }, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }

        return new PaymentResponse("Sold");
    }
}
