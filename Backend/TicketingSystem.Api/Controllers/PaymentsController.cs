using Microsoft.AspNetCore.Mvc;
using TicketingSystem.Application.DTOs;
using TicketingSystem.Application.Interfaces.Services;
using TicketingSystem.Application.UseCases.Payment;
using TicketingSystem.Application.UseCases.Reservation.GetReservation;

namespace TicketingSystem.Api.Controllers;

[ApiController]
[Route("api/v1/reservations")]
public class PaymentsController : ControllerBase
{
    private readonly IQueryHandler<GetReservationQuery, ReservationDto?> _getReservationHandler;
    private readonly ICommandHandler<ProcessPaymentCommand, PaymentResponse> _processPaymentHandler;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(
        IQueryHandler<GetReservationQuery, ReservationDto?> getReservationHandler,
        ICommandHandler<ProcessPaymentCommand, PaymentResponse> processPaymentHandler,
        ILogger<PaymentsController> logger)
    {
        _getReservationHandler = getReservationHandler;
        _processPaymentHandler = processPaymentHandler;
        _logger = logger;
    }

    /// <summary>
    /// Get reservation details
    /// </summary>
    [HttpGet("{reservationId}")]
    [ProducesResponseType(typeof(ReservationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetReservation(Guid reservationId)
    {
        var reservation = await _getReservationHandler.Handle(new GetReservationQuery(reservationId), default);
        if (reservation == null) return NotFound();
        return Ok(reservation);
    }

    /// <summary>
    /// Pay for a reservation
    /// </summary>
    [HttpPost("{reservationId}/payments")]
    [ProducesResponseType(typeof(PaymentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Pay(
        Guid reservationId,
        [FromBody] ProcessPaymentCommand command,
        CancellationToken cancellationToken)
    {
        if (reservationId != command.ReservationId)
        {
            return BadRequest(new { error = "ReservationId mismatch" });
        }

        if (string.IsNullOrWhiteSpace(command.TransactionId))
        {
            return BadRequest(new { error = "TransactionId is required" });
        }

        var result = await _processPaymentHandler.Handle(command, cancellationToken);
        return Ok(result);
    }
}
