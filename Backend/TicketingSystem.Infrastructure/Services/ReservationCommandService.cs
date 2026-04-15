using TicketingSystem.Application.DTOs;
using TicketingSystem.Application.Services;
using TicketingSystem.Domain.Entities;
using TicketingSystem.Domain.Exceptions;
using TicketingSystem.Domain.Interfaces;

namespace TicketingSystem.Infrastructure.Services;

public class ReservationCommandService : IReservationCommandService
{
    private readonly ISeatRepository _seatRepository;
    private readonly IReservationRepository _reservationRepository;
    private readonly IAuditRepository _auditRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ReservationCommandService(
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

    public async Task<ReserveSeatResponse> ReserveSeatAsync(int eventId, int sectorId, string userId, int seatId, CancellationToken cancellationToken = default)
    {
        var seat = await _seatRepository.GetByIdAsync(seatId, cancellationToken);
        if (seat == null)
        {
            throw new KeyNotFoundException($"Seat {seatId} not found");
        }

        if (seat.Status != SeatStatus.Available)
        {
            throw new BusinessException($"Seat {seatId} is not available. Current status: {seat.Status}");
        }

        var activeReservation = await _reservationRepository.GetActiveAsync(seatId, cancellationToken);
        if (activeReservation != null && !activeReservation.IsExpired)
        {
            await _auditRepository.AddAsync(new AuditLog
            {
                UserId = userId,
                Action = AuditAction.ConflictAttempt,
                ResourceType = "Seat",
                ResourceId = seatId,
                Details = $"Concurrent reservation attempt by user {userId}"
            }, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            throw new ConcurrencyException("Seat is currently reserved by another user");
        }

        var reservation = new Reservation
        {
            Seat = seat,
            SeatId = seatId,
            UserId = userId,
            ReservedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5)
        };

        seat.Status = SeatStatus.Reserved;

        await _reservationRepository.AddAsync(reservation, cancellationToken);
        await _seatRepository.UpdateAsync(seat, cancellationToken);

        await _auditRepository.AddAsync(new AuditLog
        {
            UserId = userId,
            Action = AuditAction.Reserved,
            ResourceType = "Seat",
            ResourceId = seatId,
            Details = $"Seat reserved by user {userId} until {reservation.ExpiresAt:O}"
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ReserveSeatResponse(reservation.Id, reservation.ExpiresAt);
    }
}
