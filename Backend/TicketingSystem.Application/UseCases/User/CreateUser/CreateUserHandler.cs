using TicketingSystem.Application.Interfaces.persistence;
using TicketingSystem.Application.Interfaces.Services;
using TicketingSystem.Domain.Entities;

namespace TicketingSystem.Application.UseCases.User.CreateUser;

public class CreateUserHandler : ICommandHandler<CreateUserCommand, int>
{
    private readonly IUserRepository _userRepository;
    private readonly IAuditRepository _auditRepository;
    private readonly IUnitOfWork _uow;

    public CreateUserHandler(IUserRepository userRepository, IAuditRepository auditRepository, IUnitOfWork uow)
    {
        _userRepository = userRepository;
        _auditRepository = auditRepository;
        _uow = uow;
    }

    public async Task<int> Handle(CreateUserCommand command, CancellationToken ct)
    {
        await _uow.BeginTransactionAsync(ct);
        try
        {
            var existingUser = await _userRepository.GetByEmailAsync(command.Email, ct);
            if (existingUser != null) throw new Exception("User already exists");

            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var hashPassword = Convert.ToBase64String(sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(command.Password)));

            var user = new TicketingSystem.Domain.Entities.User
            {
                Name = command.Name,
                Email = command.Email,
                PasswordHash = hashPassword,
            };

            await _userRepository.AddAsync(user, ct);
            await _uow.SaveChangesAsync(ct);

            await _auditRepository.AddAsync(new AuditLog
            {
                Action = AuditAction.Created,
                ResourceType = "User",
                ResourceId = user.Id,
                Details = $"User {user.Email} created",
                UserId = user.Id.ToString()
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
