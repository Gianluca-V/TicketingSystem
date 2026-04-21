namespace TicketingSystem.Domain.QueryFilters;

public class SectorFilter
{
    public int? EventId { get; set; }
    public string? Name { get; set; }
    public int? Take { get; set; } = 100;
    public int? Page { get; set; } = 1;
}
