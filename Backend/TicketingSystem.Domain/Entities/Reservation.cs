namespace TicketingSystem.Domain.Entities;

public class Reservation
{
    public int Id { get; set; }
    public int SeatId { get; set; }
    public required Seat Seat { get; set; }
    public required string UserId { get; set; }
    public DateTime ReservedAt { get; set; } = DateTime.UtcNow;
    public DateTime? PaidAt { get; set; }
    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddMinutes(5);
    public bool IsExpired => !PaidAt.HasValue && DateTime.UtcNow > ExpiresAt;
}
