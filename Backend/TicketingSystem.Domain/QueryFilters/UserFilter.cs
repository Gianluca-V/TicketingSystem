namespace TicketingSystem.Domain.QueryFilters;

public class UserFilter
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public int? Take { get; set; } = 100;
    public int? Page { get; set; } = 1;
}
