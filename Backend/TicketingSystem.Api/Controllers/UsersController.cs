using Microsoft.AspNetCore.Mvc;
using TicketingSystem.Application.DTOs;
using TicketingSystem.Application.Interfaces.Services;
using TicketingSystem.Application.UseCases.User.CreateUser;
using TicketingSystem.Application.UseCases.User.GetUsers;

namespace TicketingSystem.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class UsersController : ControllerBase
{
    private readonly ICommandHandler<CreateUserCommand, int> _createUserHandler;
    private readonly IQueryHandler<GetUsersQuery, IEnumerable<UserDto>> _getUsersHandler;

    public UsersController(
        ICommandHandler<CreateUserCommand, int> createUserHandler,
        IQueryHandler<GetUsersQuery, IEnumerable<UserDto>> getUsersHandler)
    {
        _createUserHandler = createUserHandler;
        _getUsersHandler = getUsersHandler;
    }

    /// <summary>
    /// Register a new user (Sign up)
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] CreateUserCommand command, CancellationToken ct)
    {
        try
        {
            var userId = await _createUserHandler.Handle(command, ct);
            return CreatedAtAction(nameof(Register), new { id = userId }, new { Id = userId });
        }
        catch (Exception ex) when (ex.Message == "User already exists")
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    /// <summary>
    /// Get all users
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<UserDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUsers(CancellationToken ct)
    {
        var users = await _getUsersHandler.Handle(new GetUsersQuery(), ct);
        return Ok(users);
    }
}
