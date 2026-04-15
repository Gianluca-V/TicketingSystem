namespace TicketingSystem.Domain.Entities;

public class AuditLog
{
    public int Id { get; set; }
    public required string UserId { get; set; }
    public AuditAction Action { get; set; }
    public required string ResourceType { get; set; }
    public int ResourceId { get; set; }
    public required string Details { get; set; }
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
}
