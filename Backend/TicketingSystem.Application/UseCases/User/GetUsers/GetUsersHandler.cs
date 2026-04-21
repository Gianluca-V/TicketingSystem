using TicketingSystem.Application.DTOs;
using TicketingSystem.Application.Interfaces.persistence;
using TicketingSystem.Application.Interfaces.Services;
using TicketingSystem.Domain.QueryFilters;

namespace TicketingSystem.Application.UseCases.User.GetUsers;

public class GetUsersHandler : IQueryHandler<GetUsersQuery, IEnumerable<UserDto>>
{
    private readonly IUserRepository _userRepository;

    public GetUsersHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<IEnumerable<UserDto>> Handle(GetUsersQuery query, CancellationToken ct)
    {
        var filter = new UserFilter
        {
            Name = query.Name,
            Email = query.Email,
            Page = query.Page,
            Take = query.Take
        };

        var users = await _userRepository.GetAllAsync(filter, ct);

        return users.Select(u => new UserDto(
            u.Id,
            u.Name,
            u.Email
        ));
    }
}
