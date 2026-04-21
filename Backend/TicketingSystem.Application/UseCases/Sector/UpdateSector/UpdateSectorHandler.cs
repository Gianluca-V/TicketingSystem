using TicketingSystem.Application.Interfaces.persistence;
using TicketingSystem.Application.Interfaces.Services;
using TicketingSystem.Domain.Entities;

namespace TicketingSystem.Application.UseCases.Sector.UpdateSector;

public class UpdateSectorHandler : ICommandHandler<UpdateSectorCommand>
{
    private readonly ISectorRepository _sectorRepository;
    private readonly IAuditRepository _auditRepository;
    private readonly IUnitOfWork _uow;

    public UpdateSectorHandler(ISectorRepository sectorRepository, IAuditRepository auditRepository, IUnitOfWork uow)
    {
        _sectorRepository = sectorRepository;
        _auditRepository = auditRepository;
        _uow = uow;
    }

    public async Task Handle(UpdateSectorCommand command, CancellationToken ct)
    {
        await _uow.BeginTransactionAsync(ct);
        try
        {
            var sector = await _sectorRepository.GetByIdAsync(command.Id, ct);
            if (sector == null) throw new Exception("Sector not found");

            if (command.Name != null) sector.Name = command.Name;
            if (command.Price.HasValue) sector.Price = command.Price.Value;
            if (command.Capacity.HasValue) sector.Capacity = command.Capacity.Value;

            await _sectorRepository.UpdateAsync(sector, ct);

            await _auditRepository.AddAsync(new AuditLog
            {
                Action = AuditAction.Updated,
                ResourceType = "Sector",
                ResourceId = sector.Id,
                Details = $"Sector {sector.Id} updated",
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
