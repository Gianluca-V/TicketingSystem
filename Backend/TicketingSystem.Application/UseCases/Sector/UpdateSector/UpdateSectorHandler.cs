using TicketingSystem.Application.Interfaces.persistence;
using TicketingSystem.Application.Interfaces.Services;
using TicketingSystem.Domain.Entities;

namespace TicketingSystem.Application.UseCases.Sector.UpdateSector;

public class UpdateSectorHandler : ICommandHandler<UpdateSectorCommand>
{
    private readonly ISectorRepository _sectorRepository;
    private readonly IAuditRepository _auditRepository;
    private readonly ICacheService _cacheService;
    private readonly IUnitOfWork _uow;

    public UpdateSectorHandler(ISectorRepository sectorRepository, IAuditRepository auditRepository, ICacheService cacheService, IUnitOfWork uow)
    {
        _sectorRepository = sectorRepository;
        _auditRepository = auditRepository;
        _cacheService = cacheService;
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
                ResourceId = sector.Id.ToString(),
                Details = $"Sector {sector.Id} updated",
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
