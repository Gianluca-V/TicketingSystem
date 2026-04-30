using TicketingSystem.Application.Interfaces.persistence;
using TicketingSystem.Application.Interfaces.Services;
using TicketingSystem.Domain.Entities;

namespace TicketingSystem.Application.UseCases.Sector.DeleteSector;

public class DeleteSectorHandler : ICommandHandler<DeleteSectorCommand>
{
    private readonly ISectorRepository _sectorRepository;
    private readonly IAuditRepository _auditRepository;
    private readonly ICacheService _cacheService;
    private readonly IUnitOfWork _uow;

    public DeleteSectorHandler(ISectorRepository sectorRepository, IAuditRepository auditRepository, ICacheService cacheService, IUnitOfWork uow)
    {
        _sectorRepository = sectorRepository;
        _auditRepository = auditRepository;
        _cacheService = cacheService;
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
                ResourceId = command.Id.ToString(),
                Details = $"Sector {command.Id} deleted",
                UserId = 0
            }, ct);

            await _uow.CommitTransactionAsync(ct);

            // Invalidate cache
            await _cacheService.RemoveAsync($"Sector:{command.Id}", ct);
            await _cacheService.RemoveByPrefixAsync("Sectors:List", ct);
            await _cacheService.RemoveByPrefixAsync("Seats:List", ct);
        }
        catch
        {
            await _uow.RollbackTransactionAsync(ct);
            throw;
        }
    }
}
