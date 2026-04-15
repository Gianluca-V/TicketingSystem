namespace TicketingSystem.Application.DTOs;

public record SeatDto(int Id, string SeatNumber, string SectorName, decimal Price, string Status);
public record ReserveSeatRequest(string UserId, int SeatId);
public record ReserveSeatResponse(int ReservationId, DateTime ExpiresAt);
public record PaymentRequest(string TransactionId);
public record PaymentResponse(string Status);
public record EventDto(int Id, string Name, DateTime Date, int SectorCount);
public record AuditLogDto(int Id, string UserId, string Action, string ResourceType, int ResourceId, string Details, DateTime OccurredAt);
