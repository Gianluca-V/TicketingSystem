namespace TicketingSystem.Domain.Entities;

public class Event
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public DateTime EventDate { get; set; }
    public string Venue { get; set; } = string.Empty;
    public string Status { get; set; }
}
