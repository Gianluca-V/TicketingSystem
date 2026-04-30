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

    public ReservationsController(
        ICommandHandler<ReserveSeatCommand, ReserveSeatResponse> reserveSeatHandler)
    {
        _reserveSeatHandler = reserveSeatHandler;
    }

    [HttpPost("seats/{seatId}/reservations")]
    public async Task<IActionResult> Create(int seatId, [FromBody] ReserveSeatCommand command, CancellationToken ct)
    {
        var reservation = await _reserveSeatHandler.Handle(new ReserveSeatCommand(seatId, command.UserId), ct);
        return Ok(reservation);
    }
}
