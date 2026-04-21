using TicketingSystem.Application.Interfaces.Services;

namespace TicketingSystem.Application.UseCases.Payment;

public record ProcessPaymentCommand(Guid ReservationId, string TransactionId) : ICommand;
