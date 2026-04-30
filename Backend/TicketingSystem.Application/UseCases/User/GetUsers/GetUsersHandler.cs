using TicketingSystem.Application.DTOs;
using TicketingSystem.Application.Interfaces.persistence;
using TicketingSystem.Application.Interfaces.Services;
using TicketingSystem.Domain.QueryFilters;

namespace TicketingSystem.Application.UseCases.User.GetUsers;

public class GetUsersHandler : IQueryHandler<GetUsersQuery, IEnumerable<UserDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly ICacheService _cacheService;

    public GetUsersHandler(IUserRepository userRepository, ICacheService cacheService)
    {
        _userRepository = userRepository;
        _cacheService = cacheService;
    }

    public async Task<IEnumerable<UserDto>> Handle(GetUsersQuery query, CancellationToken ct)
    {
        var cacheKey = $"Users:List:{query.Name}:{query.Email}:{query.Page}:{query.Take}";
        var cachedUsers = await _cacheService.GetAsync<IEnumerable<UserDto>>(cacheKey, ct);
        if (cachedUsers != null)
        {
            return cachedUsers;
        }

        var filter = new UserFilter
        {
            Name = query.Name,
            Email = query.Email,
            Page = query.Page,
            Take = query.Take
        };

        var users = await _userRepository.GetAllAsync(filter, ct);

        var result = users.Select(u => new UserDto(
            u.Id,
            u.Name,
            u.Email
        )).ToList();

        await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(10), ct);

        return result;
    }
}
