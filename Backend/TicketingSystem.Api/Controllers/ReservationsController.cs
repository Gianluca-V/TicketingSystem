using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TicketingSystem.Application.DTOs;
using TicketingSystem.Application.Interfaces.Services;
using TicketingSystem.Application.UseCases.Reservation.GetReservation;
using TicketingSystem.Application.UseCases.Seat.ReserveSeat;

namespace TicketingSystem.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1")]
public class ReservationsController : ControllerBase
{
    private readonly ICommandHandler<ReserveSeatCommand, ReserveSeatResponse> _reserveSeatHandler;
    private readonly IQueryHandler<GetReservationsQuery, IEnumerable<ReservationDto>> _getReservationsHandler;
    private readonly IQueryHandler<GetReservationByIdQuery, ReservationDto?> _getReservationByIdHandler;

    public ReservationsController(
        ICommandHandler<ReserveSeatCommand, ReserveSeatResponse> reserveSeatHandler,
        IQueryHandler<GetReservationsQuery, IEnumerable<ReservationDto>> getReservationsHandler,
        IQueryHandler<GetReservationByIdQuery, ReservationDto?> getReservationByIdHandler)
    {
        _reserveSeatHandler = reserveSeatHandler;
        _getReservationsHandler = getReservationsHandler;
        _getReservationByIdHandler = getReservationByIdHandler;
    }

    [HttpPost("seats/{seatId}/reservations")]
    public async Task<IActionResult> Create(int seatId, [FromBody] ReserveSeatCommand command, CancellationToken ct)
    {
        var reservation = await _reserveSeatHandler.Handle(new ReserveSeatCommand(seatId, command.UserId), ct);
        return Ok(reservation);
    }

    [HttpGet("reservations")]
    public async Task<IActionResult> GetReservations([FromQuery] GetReservationsQuery query, CancellationToken ct)
    {
        var reservations = await _getReservationsHandler.Handle(query, ct);
        return Ok(reservations);
    }

    [HttpGet("reservations/{id}")]
    public async Task<IActionResult> GetReservationById(Guid id, CancellationToken ct)
    {
        var reservationDto = await _getReservationByIdHandler.Handle(new GetReservationByIdQuery(id), ct);

        if (reservationDto is null)
            return NotFound();

        return Ok(reservationDto);
    }
}
