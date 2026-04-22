using Microsoft.AspNetCore.Mvc;
using TicketingSystem.Application.DTOs;
using TicketingSystem.Application.Interfaces.Services;
using TicketingSystem.Application.UseCases.Seat.ReserveSeat;

namespace TicketingSystem.Api.Controllers;

[ApiController]
[Route("api/v1/seats/{seatId}")]
public class ReservationsController : ControllerBase
{
    private readonly ICommandHandler<ReserveSeatCommand, ReserveSeatResponse> _reserveSeatHandler;
    private readonly ILogger<ReservationsController> _logger;

    public ReservationsController(
        ICommandHandler<ReserveSeatCommand, ReserveSeatResponse> reserveSeatHandler,
        ILogger<ReservationsController> logger)
    {
        _reserveSeatHandler = reserveSeatHandler;
        _logger = logger;
    }

    /// <summary>
    /// Reserve a seat
    /// </summary>
    [HttpPost("reservations")]
    [ProducesResponseType(typeof(ReserveSeatResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ReserveSeat(
        int seatId,
        [FromBody] ReserveSeatCommand command,
        CancellationToken cancellationToken)
    {
        if (seatId != command.SeatId)
        {
            return BadRequest(new { error = "SeatId mismatch" });
        }

        if (command.UserId <= 0)
        {
            return BadRequest(new { error = "Valid UserId is required" });
        }

        var result = await _reserveSeatHandler.Handle(command, cancellationToken);

        return CreatedAtAction(
            actionName: nameof(PaymentsController.GetReservation),
            controllerName: "Payments",
            routeValues: new { reservationId = result.ReservationId },
            value: result);
    }
}
