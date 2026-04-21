namespace TicketingSystem.Domain.Entities;

public class AuditLog
{
    public Guid Id { get; set; }
    public int UserId { get; set; }
    public AuditAction Action { get; set; }
    public required string ResourceType { get; set; }
    public string ResourceId { get; set; } = string.Empty;
    public required string Details { get; set; }
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
}
