using Microsoft.AspNetCore.Identity;
using TicketingSystem.Application.Interfaces.persistence;
using TicketingSystem.Application.Interfaces.Services;
using DomainUser = TicketingSystem.Domain.Entities.User;
using TicketingSystem.Domain.Entities;
using TicketingSystem.Domain.Exceptions;

namespace TicketingSystem.Application.UseCases.User.CreateUser;

public class CreateUserHandler : ICommandHandler<CreateUserCommand, int>
{
    private readonly UserManager<DomainUser> _userManager;
    private readonly IAuditRepository _auditRepository;
    private readonly IUnitOfWork _uow;

    public CreateUserHandler(UserManager<DomainUser> userManager, IAuditRepository auditRepository, IUnitOfWork uow)
    {
        _userManager = userManager;
        _auditRepository = auditRepository;
        _uow = uow;
    }

    public async Task<int> Handle(CreateUserCommand command, CancellationToken ct)
    {
        var existingUser = await _userManager.FindByEmailAsync(command.Email);
        if (existingUser != null)
        {
            throw new BusinessException("User already exists");
        }

        await _uow.BeginTransactionAsync(ct);
        try
        {
            var user = new DomainUser
            {
                UserName = command.Email,
                Email = command.Email,
                Name = command.Name
            };

            var result = await _userManager.CreateAsync(user, command.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new BusinessException($"User creation failed: {errors}");
            }

            await _auditRepository.AddAsync(new AuditLog
            {
                Action = AuditAction.Created,
                ResourceType = "User",
                ResourceId = user.Id.ToString(),
                Details = $"User {user.Email} created",
                UserId = user.Id
            }, ct);

            await _uow.CommitTransactionAsync(ct);
            return user.Id;
        }
        catch (BusinessException)
        {
            await _uow.RollbackTransactionAsync(ct);
            throw;
        }
        catch (Exception ex)
        {
            await _uow.RollbackTransactionAsync(ct);
            throw new BusinessException("An error occurred during user creation", ex);
        }
    }
}
