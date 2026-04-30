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
    
}