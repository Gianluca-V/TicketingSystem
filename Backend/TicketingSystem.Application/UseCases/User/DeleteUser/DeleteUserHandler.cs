using TicketingSystem.Application.Interfaces.persistence;
using TicketingSystem.Application.Interfaces.Services;
using TicketingSystem.Domain.Entities;

namespace TicketingSystem.Application.UseCases.User.DeleteUser;

public class DeleteUserHandler : ICommandHandler<DeleteUserCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IAuditRepository _auditRepository;
    private readonly IUnitOfWork _uow;

    public DeleteUserHandler(IUserRepository userRepository, IAuditRepository auditRepository, IUnitOfWork uow)
    {
        _userRepository = userRepository;
        _auditRepository = auditRepository;
        _uow = uow;
    }

    public async Task Handle(DeleteUserCommand command, CancellationToken ct)
    {
        await _uow.BeginTransactionAsync(ct);
        try
        {
            var user = await _userRepository.GetByIdAsync(command.Id, ct);
            if (user == null) throw new Exception("User not found");

            await _userRepository.DeleteAsync(command.Id, ct);

            await _auditRepository.AddAsync(new AuditLog
            {
                Action = AuditAction.Deleted,
                ResourceType = "User",
                ResourceId = command.Id.ToString(),
                Details = $"User {command.Id} deleted",
                UserId = 0
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
