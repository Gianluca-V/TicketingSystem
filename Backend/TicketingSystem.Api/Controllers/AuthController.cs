using Microsoft.AspNetCore.Mvc;
using TicketingSystem.Application.Interfaces.Services;
using TicketingSystem.Application.UseCases.User.Login;

namespace TicketingSystem.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly ICommandHandler<LoginCommand, string> _loginHandler;

    public AuthController(ICommandHandler<LoginCommand, string> loginHandler)
    {
        _loginHandler = loginHandler;
    }

    [HttpPost("tokens")]
    public async Task<IActionResult> Login([FromBody] LoginCommand command, CancellationToken ct)
    {
        var token = await _loginHandler.Handle(command, ct);
        return Ok(new { Token = token });
    }
}
