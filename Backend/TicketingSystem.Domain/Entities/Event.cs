namespace TicketingSystem.Domain.Entities;

public class Event
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public DateTime Date { get; set; }
    public ICollection<Sector> Sectors { get; set; } = new List<Sector>();
}
