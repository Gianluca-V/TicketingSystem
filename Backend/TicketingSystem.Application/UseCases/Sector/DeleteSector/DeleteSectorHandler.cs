using TicketingSystem.Application.Interfaces.persistence;
using TicketingSystem.Application.Interfaces.Services;
using TicketingSystem.Domain.Entities;

namespace TicketingSystem.Application.UseCases.Sector.DeleteSector;

public class DeleteSectorHandler : ICommandHandler<DeleteSectorCommand>
{
    private readonly ISectorRepository _sectorRepository;
    private readonly IAuditRepository _auditRepository;
    private readonly IUnitOfWork _uow;

    public DeleteSectorHandler(ISectorRepository sectorRepository, IAuditRepository auditRepository, IUnitOfWork uow)
    {
        _sectorRepository = sectorRepository;
        _auditRepository = auditRepository;
        _uow = uow;
    }

    public async Task Handle(DeleteSectorCommand command, CancellationToken ct)
    {
        await _uow.BeginTransactionAsync(ct);
        try
        {
            var sector = await _sectorRepository.GetByIdAsync(command.Id, ct);
            if (sector == null) throw new Exception("Sector not found");

            await _sectorRepository.DeleteAsync(command.Id, ct);

            await _auditRepository.AddAsync(new AuditLog
            {
                Action = AuditAction.Deleted,
                ResourceType = "Sector",
                ResourceId = command.Id,
                Details = $"Sector {command.Id} deleted",
                UserId = ""
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
