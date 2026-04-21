using TicketingSystem.Application.Interfaces.Services;

namespace TicketingSystem.Application.UseCases.User.CreateUser;

public class CreateUserCommand : ICommand
{
    public required string Name { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
}
