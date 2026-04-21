using TicketingSystem.Application.Interfaces.persistence;
using TicketingSystem.Application.Interfaces.Services;
using TicketingSystem.Domain.Entities;

namespace TicketingSystem.Application.UseCases.Seat.DeleteSeat;

public class DeleteSeatHandler : ICommandHandler<DeleteSeatCommand>
{
    private readonly ISeatRepository _seatRepository;
    private readonly IAuditRepository _auditRepository;
    private readonly IUnitOfWork _uow;

    public DeleteSeatHandler(ISeatRepository seatRepository, IAuditRepository auditRepository, IUnitOfWork uow)
    {
        _seatRepository = seatRepository;
        _auditRepository = auditRepository;
        _uow = uow;
    }

    public async Task Handle(DeleteSeatCommand command, CancellationToken ct)
    {
        await _uow.BeginTransactionAsync(ct);
        try
        {
            var seat = await _seatRepository.GetByIdAsync(command.Id, ct);
            if (seat == null) throw new Exception("Seat not found");

            await _seatRepository.DeleteAsync(command.Id, ct);

            await _auditRepository.AddAsync(new AuditLog
            {
                Action = AuditAction.Deleted,
                ResourceType = "Seat",
                ResourceId = command.Id,
                Details = $"Seat {command.Id} deleted",
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
