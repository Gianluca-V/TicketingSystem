using Microsoft.AspNetCore.Mvc;
using TicketingSystem.Application.Interfaces.Services;
using TicketingSystem.Application.UseCases.User.CreateUser;
using TicketingSystem.Application.UseCases.User.Login;

namespace TicketingSystem.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly ICommandHandler<LoginCommand, string> _loginHandler;
    private readonly ICommandHandler<CreateUserCommand, int> _registerHandler;

    public AuthController(
        ICommandHandler<LoginCommand, string> loginHandler,
        ICommandHandler<CreateUserCommand, int> registerHandler)
    {
        _loginHandler = loginHandler;
        _registerHandler = registerHandler;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginCommand command, CancellationToken ct)
    {
        var token = await _loginHandler.Handle(command, ct);
        return Ok(new { Token = token });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] CreateUserCommand command, CancellationToken ct)
    {
        var userId = await _registerHandler.Handle(command, ct);
        return Ok(new { Id = userId });
    }
}
