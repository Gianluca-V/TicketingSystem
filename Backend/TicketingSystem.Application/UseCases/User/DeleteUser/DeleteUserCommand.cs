using TicketingSystem.Application.Interfaces.Services;

namespace TicketingSystem.Application.UseCases.User.DeleteUser;

public class DeleteUserCommand : ICommand
{
    public int Id { get; set; }
}
