using TicketingSystem.Domain.Entities;

namespace TicketingSystem.Application.Interfaces.Services;

public interface IJwtService
{
    string GenerateToken(User user);
}
