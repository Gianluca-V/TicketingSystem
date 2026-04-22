using TicketingSystem.Application.Interfaces.Services;

namespace TicketingSystem.Application.UseCases.User.Login;

public class LoginCommand : ICommand
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
