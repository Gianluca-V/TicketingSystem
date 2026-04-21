using TicketingSystem.Application.Interfaces.Services;

namespace TicketingSystem.Application.UseCases.Seat.DeleteSeat;

public class DeleteSeatCommand : ICommand
{
    public int Id { get; set; }
}
