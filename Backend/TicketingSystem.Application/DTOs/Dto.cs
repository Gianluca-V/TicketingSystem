using TicketingSystem.Domain.Entities;

namespace TicketingSystem.Application.DTOs;

public record SeatDto(
    int Id,
    string SeatNumber,
    int SectorId,
    string SectorName,
    decimal Price,
    string Status
);

public record ReservationDto(
    Guid Id,
    int SeatId,
    string UserId,
    DateTime ReservedAt,
    DateTime ExpiresAt,
    bool IsExpired
);

public record ReserveSeatResponse(
    Guid ReservationId,
    DateTime ExpiresAt
);

public record PaymentResponse(
    string Status
);

public record EventDto(
    int Id,
    string Name,
    DateTime Date,
    string Venue,
    int SectorCount
);

public record AuditLogDto(
    Guid Id,
    int UserId,
    string Action,
    string ResourceType,
    string ResourceId,
    string Details,
    DateTime OccurredAt
);

public record UserDto(
    int Id,
    string Name,
    string Email
);

public record SectorDto(
    int Id,
    int EventId,
    string EventName,
    string Name,
    decimal Price,
    int Capacity
);

public record ReserveSeatRequest(
    int SeatId,
    int UserId
);

public record PaymentRequest(
    string TransactionId
);