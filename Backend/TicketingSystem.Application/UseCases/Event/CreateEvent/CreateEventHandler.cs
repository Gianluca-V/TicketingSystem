using TicketingSystem.Application.Interfaces.persistence;
using TicketingSystem.Application.Interfaces.Services;
using TicketingSystem.Domain.Entities;

namespace TicketingSystem.Application.UseCases.Event.CreateEvent;

public class CreateEventHandler : ICommandHandler<CreateEventCommand, int>
{
    private readonly IEventRepository _eventRepository;
    private readonly IAuditRepository _auditRepository;
    private readonly IUnitOfWork _uow;

    public CreateEventHandler(IEventRepository eventRepository, IAuditRepository auditRepository, IUnitOfWork uow)
    {
        _eventRepository = eventRepository;
        _auditRepository = auditRepository;
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
                EventDate = command.EventDate,
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
                UserId = 0
            }, ct);

            await _uow.CommitTransactionAsync(ct);
            return @event.Id;
        }
        catch
        {
            await _uow.RollbackTransactionAsync(ct);
            throw;
        }
    }
}
