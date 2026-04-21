using TicketingSystem.Application.Interfaces.Services;

namespace TicketingSystem.Application.UseCases.Seat.ReserveSeat;

public record ReserveSeatCommand(int SeatId, int UserId) : ICommand;
