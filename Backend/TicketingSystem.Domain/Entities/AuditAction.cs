namespace TicketingSystem.Domain.Entities;

public enum AuditAction
{
    Reserved = 0,
    PaymentConfirmed = 1,
    Released = 2,
    ConflictAttempt = 3,
    ExpiredLock = 4,
    Created = 5,
    Deleted = 6,
    Updated = 7,
    Login = 8
}
