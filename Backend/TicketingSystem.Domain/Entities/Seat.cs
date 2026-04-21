using System.ComponentModel.DataAnnotations;

namespace TicketingSystem.Domain.Entities;

public class Seat
{
    public int Id { get; set; }
    public int SectorId { get; set; }
    public string RowIdentifier { get; set; } = string.Empty;
    public required string SeatNumber { get; set; }
    public decimal Price { get; set; }
    public SeatStatus Status { get; set; } = SeatStatus.Available;  
    public uint Version { get; set; }
}
