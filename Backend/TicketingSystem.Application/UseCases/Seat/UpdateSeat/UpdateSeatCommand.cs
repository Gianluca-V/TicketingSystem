using TicketingSystem.Application.Interfaces.Services;
using TicketingSystem.Domain.Entities;

namespace TicketingSystem.Application.UseCases.Seat.UpdateSeat;

public class UpdateSeatCommand : ICommand
{
    public int Id { get; set; }
    public string? SeatNumber { get; set; }
    public decimal? Price { get; set; }
    public string? RowIdentifier { get; set; }
    public SeatStatus? Status { get; set; }
}
