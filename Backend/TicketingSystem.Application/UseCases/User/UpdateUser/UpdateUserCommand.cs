using TicketingSystem.Application.Interfaces.Services;

namespace TicketingSystem.Application.UseCases.User.UpdateUser;

public class UpdateUserCommand : ICommand
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }
}
