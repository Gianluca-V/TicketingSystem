using TicketingSystem.Application.DTOs;

namespace TicketingSystem.Application.Services;

public interface IPaymentCommandService
{
    Task<PaymentResponse> ProcessPaymentAsync(int reservationId, string transactionId, CancellationToken cancellationToken = default);
}
