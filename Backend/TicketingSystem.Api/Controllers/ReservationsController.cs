using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TicketingSystem.Application.DTOs;
using TicketingSystem.Application.Interfaces.Services;
using TicketingSystem.Application.UseCases.Reservation.GetReservation;
using TicketingSystem.Application.UseCases.Seat.ReserveSeat;

namespace TicketingSystem.Api.Controllers;

[ApiController]
[Authorize]
public class ReservationsController : ControllerBase
{
    private readonly ICommandHandler<ReserveSeatCommand, ReserveSeatResponse> _reserveSeatHandler;
    private readonly IQueryHandler<GetReservationQuery, ReservationDto?> _getReservationHandler;

    public ReservationsController(
        ICommandHandler<ReserveSeatCommand, ReserveSeatResponse> reserveSeatHandler,
        IQueryHandler<GetReservationQuery, ReservationDto?> getReservationHandler)
    {
        _reserveSeatHandler = reserveSeatHandler;
        _getReservationHandler = getReservationHandler;
    }

    /// <summary>
    /// Create a temporary reservation for a specific seat
    /// </summary>
    [HttpPost("api/v1/seats/{seatId}/reservations")]
    [ProducesResponseType(typeof(ReserveSeatResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ReserveSeat(int seatId, [FromBody] ReserveSeatCommand command, CancellationToken cancellationToken)
    {
        var result = await _reserveSeatHandler.Handle(command with { SeatId = seatId }, cancellationToken);

        return CreatedAtAction(nameof(GetReservation), new { reservationId = result.ReservationId }, result);
    }

    /// <summary>S
    /// Get details of a specific reservation
    /// </summary>
    [HttpGet("api/v1/reservations/{reservationId}")]
    [ProducesResponseType(typeof(ReservationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetReservation(Guid reservationId)
    {
        var reservation = await _getReservationHandler.Handle(new GetReservationQuery(reservationId), default);
        if (reservation == null) return NotFound();
        return Ok(reservation);
    }
}
