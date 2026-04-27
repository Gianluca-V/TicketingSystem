using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TicketingSystem.Application.DTOs;
using TicketingSystem.Application.Interfaces.Services;
using TicketingSystem.Application.UseCases.User.CreateUser;
using TicketingSystem.Application.UseCases.User.DeleteUser;
using TicketingSystem.Application.UseCases.User.GetUsers;
using TicketingSystem.Application.UseCases.User.UpdateUser;

namespace TicketingSystem.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly ICommandHandler<CreateUserCommand, int> _createUserHandler;
    private readonly ICommandHandler<UpdateUserCommand> _updateUserHandler;
    private readonly ICommandHandler<DeleteUserCommand> _deleteUserHandler;
    private readonly IQueryHandler<GetUsersQuery, IEnumerable<UserDto>> _getUsersHandler;

    public UsersController(
        ICommandHandler<CreateUserCommand, int> createUserHandler,
        ICommandHandler<UpdateUserCommand> updateUserHandler,
        ICommandHandler<DeleteUserCommand> deleteUserHandler,
        IQueryHandler<GetUsersQuery, IEnumerable<UserDto>> getUsersHandler)
    {
        _createUserHandler = createUserHandler;
        _updateUserHandler = updateUserHandler;
        _deleteUserHandler = deleteUserHandler;
        _getUsersHandler = getUsersHandler;
    }

    /// <summary>
    /// Create a new user (Admin)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserCommand command, CancellationToken ct)
    {
        var userId = await _createUserHandler.Handle(command, ct);
        return CreatedAtAction(nameof(GetUsers), new { id = userId }, new { Id = userId });
    }

    /// <summary>
    /// Update an existing user
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserCommand command, CancellationToken ct)
    {
        command.Id = id;
        await _updateUserHandler.Handle(command, ct);
        return NoContent();
    }

    /// <summary>
    /// Delete a user
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _deleteUserHandler.Handle(new DeleteUserCommand { Id = id }, ct);
        return NoContent();
    }

    /// <summary>
    /// Get all users
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetUsers([FromQuery] GetUsersQuery query, CancellationToken ct)
    {
        var users = await _getUsersHandler.Handle(query, ct);
        return Ok(users);
    }
}
