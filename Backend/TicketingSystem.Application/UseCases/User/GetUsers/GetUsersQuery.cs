using TicketingSystem.Application.DTOs;
using TicketingSystem.Application.Interfaces.Services;

namespace TicketingSystem.Application.UseCases.User.GetUsers;

public class GetUsersQuery : IQuery<IEnumerable<UserDto>>
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public int Page { get; set; } = 1;
    public int Take { get; set; } = 100;
}
