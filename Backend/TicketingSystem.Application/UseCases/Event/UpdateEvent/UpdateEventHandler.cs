using TicketingSystem.Application.Interfaces.persistence;
using TicketingSystem.Application.Interfaces.Services;
using TicketingSystem.Domain.Entities;

namespace TicketingSystem.Application.UseCases.Event.UpdateEvent;

public class UpdateEventHandler : ICommandHandler<UpdateEventCommand>
{
    private readonly IEventRepository _eventRepository;
    private readonly IAuditRepository _auditRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _uow;

    public UpdateEventHandler(IEventRepository eventRepository, IAuditRepository auditRepository, ICurrentUserService currentUserService, IUnitOfWork uow)
    {
        _eventRepository = eventRepository;
        _auditRepository = auditRepository;
        _currentUserService = currentUserService;
        _uow = uow;
    }

    public async Task Handle(UpdateEventCommand command, CancellationToken ct)
    {
        await _uow.BeginTransactionAsync(ct);
        try
        {
            var @event = await _eventRepository.GetByIdAsync(command.Id, ct);
            if (@event == null) throw new Exception("Event not found");

            if (command.Name != null) @event.Name = command.Name;
            if (command.Date.HasValue) @event.EventDate = DateTime.SpecifyKind(command.Date.Value, DateTimeKind.Utc);
            if (command.Venue != null) @event.Venue = command.Venue;
            if (command.Status != null) @event.Status = command.Status;

            await _eventRepository.UpdateAsync(@event, ct);
            
            await _auditRepository.AddAsync(new AuditLog
            {
                Action = AuditAction.Updated,
                ResourceType = "Event",
                ResourceId = @event.Id.ToString(),
                Details = $"Event {@event.Id} updated",
                UserId = _currentUserService.UserId ?? 0
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
