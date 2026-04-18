using Microsoft.AspNetCore.Mvc;
using TicketingSystem.Application.DTOs;
using TicketingSystem.Application.Interfaces.Services;

namespace TicketingSystem.Api.Controllers;

[ApiController]
[Route("api/v1/events/{eventId}/sectors/{sectorId}")]
public class ReservationsController : ControllerBase
{
    //private readonly IReservationCommandService _reservationCommandService;
    private readonly ILogger<ReservationsController> _logger;

    public ReservationsController(
        IReservationCommandService reservationCommandService,
        ILogger<ReservationsController> logger)
    {
        //_reservationCommandService = reservationCommandService;
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
        int eventId,
        int sectorId,
        [FromBody] ReserveSeatRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.UserId))
        {
            return BadRequest(new { error = "UserId is required" });
        }

        if (request.SeatId <= 0)
        {
            return BadRequest(new { error = "Valid SeatId is required" });
        }

        /*var result = await _reservationCommandService.ReserveSeatAsync(
            eventId, sectorId, request.UserId, request.SeatId, cancellationToken);

        return CreatedAtAction(
            actionName: nameof(PaymentsController.GetReservation),
            controllerName: "Payments",
            routeValues: new { reservationId = result.ReservationId },
            value: result);*/
        return Ok();
    }
}
