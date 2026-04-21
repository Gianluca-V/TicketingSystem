namespace TicketingSystem.Domain.Entities;

public class Reservation
{
    public Guid Id { get; set; }
    public int SeatId { get; set; }
    public int UserId { get; set; } // It was int in some places and string in others, let's keep it int as per User.cs
    public DateTime ReservedAt { get; set; } = DateTime.UtcNow;
    public DateTime? PaidAt { get; set; }
    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddMinutes(5);
    public bool IsExpired => !PaidAt.HasValue && DateTime.UtcNow > ExpiresAt;
}
