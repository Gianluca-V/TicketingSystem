using TicketingSystem.Application.DTOs;
using TicketingSystem.Application.Interfaces.Services;

namespace TicketingSystem.Application.UseCases.Reservation.GetReservation;

public record GetReservationByIdQuery(Guid ReservationId) : IQuery<ReservationDto?>;
