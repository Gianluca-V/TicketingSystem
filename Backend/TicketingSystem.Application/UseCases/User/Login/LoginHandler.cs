using Microsoft.AspNetCore.Identity;
using TicketingSystem.Application.Interfaces.persistence;
using TicketingSystem.Application.Interfaces.Services;
using DomainUser = TicketingSystem.Domain.Entities.User;
using TicketingSystem.Domain.Entities;
using TicketingSystem.Domain.Exceptions;

namespace TicketingSystem.Application.UseCases.User.Login;

public class LoginHandler : ICommandHandler<LoginCommand, string>
{
    private readonly UserManager<DomainUser> _userManager;
    private readonly IJwtService _jwtService;
    private readonly IAuditRepository _auditRepository;
    private readonly IUnitOfWork _uow;

    public LoginHandler(
        UserManager<DomainUser> userManager,
        IJwtService jwtService,
        IAuditRepository auditRepository,
        IUnitOfWork uow)
    {
        _userManager = userManager;
        _jwtService = jwtService;
        _auditRepository = auditRepository;
        _uow = uow;
    }

    public async Task<string> Handle(LoginCommand command, CancellationToken ct)
    {
        var user = await _userManager.FindByEmailAsync(command.Email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, command.Password))
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
