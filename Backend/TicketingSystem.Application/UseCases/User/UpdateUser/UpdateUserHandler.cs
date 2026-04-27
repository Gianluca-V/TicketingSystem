using Microsoft.AspNetCore.Identity;
using TicketingSystem.Application.Interfaces.persistence;
using TicketingSystem.Application.Interfaces.Services;
using DomainUser = TicketingSystem.Domain.Entities.User;
using TicketingSystem.Domain.Entities;

namespace TicketingSystem.Application.UseCases.User.UpdateUser;

public class UpdateUserHandler : ICommandHandler<UpdateUserCommand>
{
    private readonly UserManager<DomainUser> _userManager;
    private readonly IAuditRepository _auditRepository;
    private readonly IUnitOfWork _uow;

    public UpdateUserHandler(UserManager<DomainUser> userManager, IAuditRepository auditRepository, IUnitOfWork uow)
    {
        _userManager = userManager;
        _auditRepository = auditRepository;
        _uow = uow;
    }

    public async Task Handle(UpdateUserCommand command, CancellationToken ct)
    {
        await _uow.BeginTransactionAsync(ct);
        try
        {
            var user = await _userManager.FindByIdAsync(command.Id.ToString());
            if (user == null) throw new Exception("User not found");

            if (command.Name != null) user.Name = command.Name;
            if (command.Email != null)
            {
                user.Email = command.Email;
                user.UserName = command.Email;
            }

            if (command.Password != null)
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                await _userManager.ResetPasswordAsync(user, token, command.Password);
            }

            await _userManager.UpdateAsync(user);

            await _auditRepository.AddAsync(new AuditLog
            {
                Action = AuditAction.Updated,
                ResourceType = "User",
                ResourceId = user.Id.ToString(),
                Details = $"User {user.Id} updated",
                UserId = user.Id
            }, ct);

            await _uow.CommitTransactionAsync(ct);
        }
        catch
        {
            await _uow.RollbackTransactionAsync(ct);
            throw;
        }
    }
}
