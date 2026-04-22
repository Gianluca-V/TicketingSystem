using TicketingSystem.Application.Interfaces.persistence;
using TicketingSystem.Application.Interfaces.Services;
using TicketingSystem.Domain.Entities;

namespace TicketingSystem.Application.UseCases.User.CreateUser;

public class CreateUserHandler : ICommandHandler<CreateUserCommand, int>
{
    private readonly IUserRepository _userRepository;
    private readonly IAuditRepository _auditRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _uow;

    public CreateUserHandler(IUserRepository userRepository, IAuditRepository auditRepository, IPasswordHasher passwordHasher, IUnitOfWork uow)
    {
        _userRepository = userRepository;
        _auditRepository = auditRepository;
        _passwordHasher = passwordHasher;
        _uow = uow;
    }

    public async Task<int> Handle(CreateUserCommand command, CancellationToken ct)
    {
        await _uow.BeginTransactionAsync(ct);
        try
        {
            var existingUser = await _userRepository.GetByEmailAsync(command.Email, ct);
            if (existingUser != null) throw new Exception("User already exists");

            var user = new TicketingSystem.Domain.Entities.User
            {
                Name = command.Name,
                Email = command.Email,
                PasswordHash = _passwordHasher.Hash(command.Password)
            };

            await _userRepository.AddAsync(user, ct);
            await _uow.SaveChangesAsync(ct);

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
        catch
        {
            await _uow.RollbackTransactionAsync(ct);
            throw;
        }
    }
}
