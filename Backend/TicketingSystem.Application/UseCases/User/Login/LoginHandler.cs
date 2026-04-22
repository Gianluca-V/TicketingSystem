using TicketingSystem.Application.Interfaces.persistence;
using TicketingSystem.Application.Interfaces.Services;
using TicketingSystem.Domain.Entities;
using TicketingSystem.Domain.Exceptions;

namespace TicketingSystem.Application.UseCases.User.Login;

public class LoginHandler : ICommandHandler<LoginCommand, string>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;
    private readonly IAuditRepository _auditRepository;
    private readonly IUnitOfWork _uow;

    public LoginHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtService jwtService,
        IAuditRepository auditRepository,
        IUnitOfWork uow)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
        _auditRepository = auditRepository;
        _uow = uow;
    }

    public async Task<string> Handle(LoginCommand command, CancellationToken ct)
    {
        var user = await _userRepository.GetByEmailAsync(command.Email, ct);
        if (user == null || !_passwordHasher.Verify(command.Password, user.PasswordHash))
        {
            throw new BusinessException("Invalid credentials");
        }

        var token = _jwtService.GenerateToken(user);

        await _auditRepository.AddAsync(new AuditLog
        {
            Action = AuditAction.Login,
            ResourceType = "User",
            ResourceId = user.Id.ToString(),
            Details = $"User {user.Email} logged in",
            UserId = user.Id
        }, ct);
        
        await _uow.SaveChangesAsync(ct);

        return token;
    }
}
