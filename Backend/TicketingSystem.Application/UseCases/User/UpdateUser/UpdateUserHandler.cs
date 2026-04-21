using TicketingSystem.Application.Interfaces.persistence;
using TicketingSystem.Application.Interfaces.Services;
using TicketingSystem.Domain.Entities;

namespace TicketingSystem.Application.UseCases.User.UpdateUser;

public class UpdateUserHandler : ICommandHandler<UpdateUserCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IAuditRepository _auditRepository;
    private readonly IUnitOfWork _uow;

    public UpdateUserHandler(IUserRepository userRepository, IAuditRepository auditRepository, IUnitOfWork uow)
    {
        _userRepository = userRepository;
        _auditRepository = auditRepository;
        _uow = uow;
    }

    public async Task Handle(UpdateUserCommand command, CancellationToken ct)
    {
        await _uow.BeginTransactionAsync(ct);
        try
        {
            var user = await _userRepository.GetByIdAsync(command.Id, ct);
            if (user == null) throw new Exception("User not found");

            if (command.Name != null) user.Name = command.Name;
            if (command.Email != null) user.Email = command.Email;
            if (command.Password != null) user.PasswordHash = command.Password; // hash it

            await _userRepository.UpdateAsync(user, ct);

            await _auditRepository.AddAsync(new AuditLog
            {
                Action = AuditAction.Updated,
                ResourceType = "User",
                ResourceId = user.Id,
                Details = $"User {user.Id} updated",
                UserId = user.Id.ToString()
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
