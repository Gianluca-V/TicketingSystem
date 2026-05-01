using TicketingSystem.Application.Interfaces.persistence;
using TicketingSystem.Application.Interfaces.Services;
using TicketingSystem.Domain.Entities;

namespace TicketingSystem.Application.UseCases.Event.UpdateEvent;

public class UpdateEventHandler : ICommandHandler<UpdateEventCommand>
{
    private readonly IEventRepository _eventRepository;
    private readonly IAuditRepository _auditRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICacheService _cacheService;
    private readonly IUnitOfWork _uow;

    public UpdateEventHandler(IEventRepository eventRepository, IAuditRepository auditRepository, ICurrentUserService currentUserService, ICacheService cacheService, IUnitOfWork uow)
    {
        _eventRepository = eventRepository;
        _auditRepository = auditRepository;
        _currentUserService = currentUserService;
        _cacheService = cacheService;
        _uow = uow;
    }

    public async Task Handle(UpdateEventCommand command, CancellationToken ct)
    {
        await _uow.BeginTransactionAsync(ct);

        var @event = await _eventRepository.GetByIdAsync(command.Id, ct);
        if (@event == null)
            throw new Exception("Event not found");

        @event.Name = command.Name;

        if (command.Date is DateTime date)
            @event.EventDate = DateTime.SpecifyKind(date, DateTimeKind.Utc);

        @event.Venue = command.Venue;
        @event.Status = command.Status;

        await _eventRepository.UpdateAsync(@event, ct);

        await _auditRepository.AddAsync(new AuditLog
        {
            Action = AuditAction.Updated,
            ResourceType = "Event",
            ResourceId = @event.Id.ToString(),
            Details = $"Event {@event.Name} updated",
            UserId = _currentUserService.UserId ?? 0
        }, ct);

        await _uow.CommitTransactionAsync(ct);

        await _cacheService.RemoveAsync($"Event:{command.Id}", ct);
        await _cacheService.RemoveByPrefixAsync("Events:List", ct);
        await _cacheService.RemoveByPrefixAsync("AuditLogs:List", ct);
    }
}
