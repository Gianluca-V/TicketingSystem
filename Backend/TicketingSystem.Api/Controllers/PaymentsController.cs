using Microsoft.AspNetCore.Mvc;
using TicketingSystem.Application.DTOs;
using TicketingSystem.Application.Interfaces.Services;

namespace TicketingSystem.Api.Controllers;

[ApiController]
[Route("api/v1/reservations")]
public class PaymentsController : ControllerBase
{
    //private readonly IPaymentCommandService _paymentCommandService;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(
        IPaymentCommandService paymentCommandService,
        ILogger<PaymentsController> logger)
    {
        //_paymentCommandService = paymentCommandService;
        _logger = logger;
    }

    /// <summary>
    /// Get reservation details
    /// </summary>
    [HttpGet("{reservationId}")]
    [ProducesResponseType(typeof(ReserveSeatResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetReservation(int reservationId)
    {
        return Ok();
    }

    /// <summary>
    /// Pay for a reservation
    /// </summary>
    [HttpPost("{reservationId}/pay")]
    [ProducesResponseType(typeof(PaymentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Pay(
        int reservationId,
        [FromBody] PaymentRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.TransactionId))
        {
            return BadRequest(new { error = "TransactionId is required" });
        }

        /*var result = await _paymentCommandService.ProcessPaymentAsync(
            reservationId, request.TransactionId, cancellationToken);

        return Ok(result);*/
        return Ok();
    }
}
