using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TicketingSystem.Application.DTOs;
using TicketingSystem.Application.Interfaces.Services;
using TicketingSystem.Application.UseCases.Payment;

namespace TicketingSystem.Api.Controllers;

[ApiController]
[Route("api/v1/reservations/{reservationId}/[controller]")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly ICommandHandler<ProcessPaymentCommand, PaymentResponse> _processPaymentHandler;

    public PaymentsController(ICommandHandler<ProcessPaymentCommand, PaymentResponse> processPaymentHandler)
    {
        _processPaymentHandler = processPaymentHandler;
    }

    /// <summary>
    /// Process a payment for a specific reservation
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(PaymentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Pay(Guid reservationId, [FromBody] ProcessPaymentCommand command, CancellationToken cancellationToken)
    {
        var result = await _processPaymentHandler.Handle(command with { ReservationId = reservationId }, cancellationToken);
        return Ok(result);
    }
}
