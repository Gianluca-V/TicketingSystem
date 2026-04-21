using TicketingSystem.Application.Interfaces.persistence;
using TicketingSystem.Application.Interfaces.Services;
using TicketingSystem.Domain.Entities;

namespace TicketingSystem.Application.UseCases.Event.DeleteEvent;

public class DeleteEventHandler : ICommandHandler<DeleteEventCommand>
{
    private readonly IEventRepository _eventRepository;
    private readonly IAuditRepository _auditRepository;
    private readonly IUnitOfWork _uow;

    public DeleteEventHandler(IEventRepository eventRepository, IAuditRepository auditRepository, IUnitOfWork uow)
    {
        _eventRepository = eventRepository;
        _auditRepository = auditRepository;
        _uow = uow;
    }

    public async Task Handle(DeleteEventCommand command, CancellationToken ct)
    {
        await _uow.BeginTransactionAsync(ct);
        try
        {
            var @event = await _eventRepository.GetByIdAsync(command.Id, ct);
            if (@event == null) throw new Exception("Event not found");

            await _eventRepository.DeleteAsync(command.Id, ct);

            await _auditRepository.AddAsync(new AuditLog
            {
                Action = AuditAction.Deleted,
                ResourceType = "Event",
                ResourceId = command.Id,
                Details = $"Event {command.Id} deleted",
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
