using TicketingSystem.Application.Interfaces.persistence;
using TicketingSystem.Application.Interfaces.Services;
using TicketingSystem.Domain.Entities;

namespace TicketingSystem.Application.UseCases.Sector.CreateSector;

public class CreateSectorHandler : ICommandHandler<CreateSectorCommand, int>
{
    private readonly ISectorRepository _sectorRepository;
    private readonly IEventRepository _eventRepository;
    private readonly IAuditRepository _auditRepository;
    private readonly ICacheService _cacheService;
    private readonly IUnitOfWork _uow;

    public CreateSectorHandler(ISectorRepository sectorRepository, IEventRepository eventRepository, IAuditRepository auditRepository, ICacheService cacheService, IUnitOfWork uow)
    {
        _sectorRepository = sectorRepository;
        _eventRepository = eventRepository;
        _auditRepository = auditRepository;
        _cacheService = cacheService;
        _uow = uow;
    }

    public async Task<int> Handle(CreateSectorCommand command, CancellationToken ct)
    {
        await _uow.BeginTransactionAsync(ct);
        try
        {
            var @event = await _eventRepository.GetByIdAsync(command.EventId, ct);
            if (@event == null) throw new Exception("Event not found");

            var sector = new TicketingSystem.Domain.Entities.Sector
            {
                EventId = command.EventId,
                Event = @event,
                Name = command.Name,
                Price = command.Price,
                Capacity = command.Capacity
            };

            await _sectorRepository.AddAsync(sector, ct);
            await _uow.SaveChangesAsync(ct);

            await _auditRepository.AddAsync(new AuditLog
            {
                Action = AuditAction.Created,
                ResourceType = "Sector",
                ResourceId = sector.Id.ToString(),
                Details = $"Sector {sector.Name} created for Event {command.EventId}",
                UserId = 0
            }, ct);

            await _uow.CommitTransactionAsync(ct);

            // Invalidate cache
            await _cacheService.RemoveByPrefixAsync("Sectors:List", ct);
            await _cacheService.RemoveByPrefixAsync("Seats:List", ct);

            return sector.Id;
        }
        catch
        {
            await _uow.RollbackTransactionAsync(ct);
            throw;
        }
    }
}
