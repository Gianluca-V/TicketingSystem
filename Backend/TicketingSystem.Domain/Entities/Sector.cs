namespace TicketingSystem.Domain.Entities;

public class Sector
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public required Event Event { get; set; }
    public required string Name { get; set; }
    public decimal Price { get; set; }
    public int Capacity { get; set; }
}
