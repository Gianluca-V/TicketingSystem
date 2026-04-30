using Microsoft.AspNetCore.Identity;
using TicketingSystem.Application.Interfaces.persistence;
using TicketingSystem.Application.Interfaces.Services;
using DomainUser = TicketingSystem.Domain.Entities.User;
using TicketingSystem.Domain.Entities;

namespace TicketingSystem.Application.UseCases.User.DeleteUser;

public class DeleteUserHandler : ICommandHandler<DeleteUserCommand>
{
    private readonly UserManager<DomainUser> _userManager;
    private readonly IAuditRepository _auditRepository;
    private readonly ICacheService _cacheService;
    private readonly IUnitOfWork _uow;

    public DeleteUserHandler(UserManager<DomainUser> userManager, IAuditRepository auditRepository, ICacheService cacheService, IUnitOfWork uow)
    {
        _userManager = userManager;
        _auditRepository = auditRepository;
        _cacheService = cacheService;
        _uow = uow;
    }

    public async Task Handle(DeleteUserCommand command, CancellationToken ct)
    {
        await _uow.BeginTransactionAsync(ct);
        try
        {
            var user = await _userManager.FindByIdAsync(command.Id.ToString());
            if (user == null) throw new Exception("User not found");

            await _userManager.DeleteAsync(user);

            await _auditRepository.AddAsync(new AuditLog
            {
                Action = AuditAction.Deleted,
                ResourceType = "User",
                ResourceId = command.Id.ToString(),
                Details = $"User {command.Id} deleted",
                UserId = 0
            }, ct);

            await _uow.CommitTransactionAsync(ct);

            // Invalidate cache
            await _cacheService.RemoveByPrefixAsync("Users:List", ct);
        }
        catch
        {
            await _uow.RollbackTransactionAsync(ct);
            throw;
        }
    }
}
