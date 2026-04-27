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
    private readonly IQueryHandler<GetReservationByIdQuery, ReservationDto?> _getReservationHandler;
    private readonly IQueryHandler<GetReservationsQuery, IEnumerable<ReservationDto>> _getReservationsHandler;

    public ReservationsController(
        ICommandHandler<ReserveSeatCommand, ReserveSeatResponse> reserveSeatHandler,
        IQueryHandler<GetReservationByIdQuery, ReservationDto?> getReservationHandler,
        IQueryHandler<GetReservationsQuery, IEnumerable<ReservationDto>> getReservationsHandler)
    {
        _reserveSeatHandler = reserveSeatHandler;
        _getReservationHandler = getReservationHandler;
        _getReservationsHandler = getReservationsHandler;
    }

    /// <summary>
    /// Create temporary reservation for a seat
    /// POST /api/v1/seats/{seatId}/reservations
    /// </summary>
    [HttpPost("seats/{seatId}/reservations")]
    [ProducesResponseType(typeof(ReserveSeatResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ReserveSeat(
        int seatId,
        [FromBody] ReserveSeatCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _reserveSeatHandler.Handle(
            command with { SeatId = seatId },
            cancellationToken);

        return CreatedAtAction(
            nameof(GetReservation),
            new { reservationId = result.ReservationId },
            result);
    }

    /// <summary>
    /// Get reservation by id
    /// GET /api/v1/reservations/{reservationId}
    /// </summary>
    [HttpGet("reservations/{reservationId:guid}")]
    [ProducesResponseType(typeof(ReservationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetReservation(
        Guid reservationId,
        CancellationToken cancellationToken)
    {
        var reservation = await _getReservationHandler.Handle(
            new GetReservationByIdQuery(reservationId),
            cancellationToken);

        if (reservation is null)
            return NotFound();

        return Ok(reservation);
    }

    /// <summary>
    /// List all reservations
    /// GET /api/v1/reservations
    /// </summary>
    [HttpGet("reservations")]
    [ProducesResponseType(typeof(IEnumerable<ReservationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReservations(
        [FromQuery] string? userId,
        [FromQuery] int? seatId,
        [FromQuery] bool? expired,
        [FromQuery] int page = 1,
        [FromQuery] int take = 100,
        CancellationToken cancellationToken = default)
    {
        var result = await _getReservationsHandler.Handle(
            new GetReservationsQuery
            {
                UserId = userId,
                SeatId = seatId,
                Expired = expired,
                Page = page,
                Take = take
            },
            cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// List reservations of a seat
    /// GET /api/v1/seats/{seatId}/reservations
    /// </summary>
    [HttpGet("seats/{seatId}/reservations")]
    [ProducesResponseType(typeof(IEnumerable<ReservationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReservationsBySeat(
        int seatId,
        [FromQuery] int page = 1,
        [FromQuery] int take = 100,
        CancellationToken cancellationToken = default)
    {
        var result = await _getReservationsHandler.Handle(
            new GetReservationsQuery
            {
                SeatId = seatId,
                Page = page,
                Take = take
            },
            cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// List reservations of a user
    /// GET /api/v1/users/{userId}/reservations
    /// </summary>
    [HttpGet("users/{userId}/reservations")]
    [ProducesResponseType(typeof(IEnumerable<ReservationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReservationsByUser(
        string userId,
        [FromQuery] int page = 1,
        [FromQuery] int take = 100,
        CancellationToken cancellationToken = default)
    {
        var result = await _getReservationsHandler.Handle(
            new GetReservationsQuery
            {
                UserId = userId,
                Page = page,
                Take = take
            },
            cancellationToken);

        return Ok(result);
    }
}