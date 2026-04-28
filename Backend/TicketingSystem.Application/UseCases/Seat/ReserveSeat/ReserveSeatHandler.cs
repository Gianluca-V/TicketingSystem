using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicketingSystem.Application.DTOs;
using TicketingSystem.Application.Interfaces.persistence;
using TicketingSystem.Application.Interfaces.Services;
using TicketingSystem.Domain.Entities;
using TicketingSystem.Domain.Exceptions;

namespace TicketingSystem.Application.UseCases.Seat.ReserveSeat
{
    public class ReserveSeatHandler
    : ICommandHandler<ReserveSeatCommand, ReserveSeatResponse>
    {
        private readonly ISeatRepository _seatRepository;
        private readonly IReservationRepository _reservationRepository;
        private readonly IAuditRepository _auditRepository;
        private readonly IUnitOfWork _uow;

        public ReserveSeatHandler(
            ISeatRepository seatRepository,
            IReservationRepository reservationRepository,
            IAuditRepository auditRepository,
            IUnitOfWork uow)
        {
            _seatRepository = seatRepository;
            _reservationRepository = reservationRepository;
            _auditRepository = auditRepository;
            _uow = uow;
        }

        public async Task<ReserveSeatResponse> Handle(ReserveSeatCommand cmd, CancellationToken ct)
        {
            await _uow.BeginTransactionAsync(ct);

            try
            {
                var seat = await _seatRepository.GetByIdAsync(cmd.SeatId, ct);

                if (seat == null)
                    throw new KeyNotFoundException("Seat not found");

                if (seat.Status != SeatStatus.Available)
                    throw new ConflictException("Seat not available");

                seat.Status = SeatStatus.Reserved;

                var reservation = new TicketingSystem.Domain.Entities.Reservation
                {
                    SeatId = seat.Id,
                    UserId = cmd.UserId
                };

                await _reservationRepository.AddAsync(reservation, ct);
                await _seatRepository.UpdateAsync(seat, ct);

                // ✅ AUDIT OK
                await _auditRepository.AddAsync(new AuditLog
                {
                    UserId = cmd.UserId,
                    Action = AuditAction.Reserved,
                    ResourceType = "Seat",
                    ResourceId = seat.Id.ToString(),
                    Details = "Seat reserved successfully"
                }, ct);

                await _uow.CommitTransactionAsync(ct);

                return new ReserveSeatResponse(
                    reservation.Id,
                    reservation.ExpiresAt
                );
            }
            catch (DbUpdateConcurrencyException)
            {
                await _uow.RollbackTransactionAsync(ct);

                await _auditRepository.AddAsync(new AuditLog
                {
                    UserId = cmd.UserId,
                    Action = AuditAction.ConflictAttempt,
                    ResourceType = "Seat",
                    ResourceId = cmd.SeatId.ToString(),
                    Details = "Concurrency conflict"
                }, ct);

                throw new ConflictException("Seat already taken");
            }
            catch
            {
                await _uow.RollbackTransactionAsync(ct);
                throw;
            }
        }
    }
}
