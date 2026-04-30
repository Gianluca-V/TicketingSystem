using TicketingSystem.Application.Interfaces.persistence;
using TicketingSystem.Application.Interfaces.Services;
using TicketingSystem.Domain.Entities;

namespace TicketingSystem.Application.UseCases.Event.CreateEvent;

public class CreateEventHandler : ICommandHandler<CreateEventCommand, int>
{
    private readonly IEventRepository _eventRepository;
    private readonly IAuditRepository _auditRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICacheService _cacheService;
    private readonly IUnitOfWork _uow;

    public CreateEventHandler(IEventRepository eventRepository, IAuditRepository auditRepository, ICurrentUserService currentUserService, ICacheService cacheService, IUnitOfWork uow)
    {
        _eventRepository = eventRepository;
        _auditRepository = auditRepository;
        _currentUserService = currentUserService;
        _cacheService = cacheService;
        _uow = uow;
    }

    public async Task<int> Handle(CreateEventCommand command, CancellationToken ct)
    {
        await _uow.BeginTransactionAsync(ct);
        try
        {
            var @event = new TicketingSystem.Domain.Entities.Event
            {
                Name = command.Name,
                EventDate = DateTime.SpecifyKind(command.Date, DateTimeKind.Utc),
                Venue = command.Venue,
                Status = command.Status
            };

            await _eventRepository.AddAsync(@event, ct);
            await _uow.SaveChangesAsync(ct); // To get the Id

            await _auditRepository.AddAsync(new AuditLog
            {
                Action = AuditAction.Created,
                ResourceType = "Event",
                ResourceId = @event.Id.ToString(),
                Details = $"Event {@event.Name} created",
                UserId = _currentUserService.UserId ?? 0
            }, ct);

            await _uow.CommitTransactionAsync(ct);

            // Invalidate cache
            await _cacheService.RemoveByPrefixAsync("Events:List", ct);
            await _cacheService.RemoveByPrefixAsync("AuditLogs:List", ct);

            return @event.Id;
        }        catch
        {
            await _uow.RollbackTransactionAsync(ct);
            throw;
        }
    }
}
