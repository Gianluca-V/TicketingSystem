using TicketingSystem.Application.Interfaces.Services;

namespace TicketingSystem.Application.UseCases.Seat.CreateSeat;

public class CreateSeatsBulkCommand : ICommand
{
    public int EventId { get; set; }
    public int SectorId { get; set; }
    public List<SeatItem> Seats { get; set; } = new();
}

public class SeatItem
{
    public required string SeatNumber { get; set; }
    public decimal Price { get; set; }
    public string RowIdentifier { get; set; } = string.Empty;
}
