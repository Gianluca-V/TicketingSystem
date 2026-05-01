using TicketingSystem.Application.Interfaces.persistence;
using TicketingSystem.Application.Interfaces.Services;
using TicketingSystem.Domain.Entities;

namespace TicketingSystem.Application.UseCases.Seat.UpdateSeat;

public class UpdateSeatHandler : ICommandHandler<UpdateSeatCommand>
{
    private readonly ISeatRepository _seatRepository;
    private readonly IAuditRepository _auditRepository;
    private readonly ICacheService _cacheService;
    private readonly IUnitOfWork _uow;

    public UpdateSeatHandler(ISeatRepository seatRepository, IAuditRepository auditRepository, ICacheService cacheService, IUnitOfWork uow)
    {
        _seatRepository = seatRepository;
        _auditRepository = auditRepository;
        _cacheService = cacheService;
        _uow = uow;
    }

    public async Task Handle(UpdateSeatCommand command, CancellationToken ct)
    {
        await _uow.BeginTransactionAsync(ct);
        try
        {
            var seat = await _seatRepository.GetByIdAsync(command.Id, ct);
            if (seat == null) throw new Exception("Seat not found");

            if (command.SeatNumber != null) seat.SeatNumber = command.SeatNumber;
            if (command.Price.HasValue) seat.Price = command.Price.Value;
            if (command.RowIdentifier != null) seat.RowIdentifier = command.RowIdentifier;
            if (command.Status.HasValue) seat.Status = command.Status.Value;

            await _seatRepository.UpdateAsync(seat, ct);

            await _auditRepository.AddAsync(new AuditLog
            {
                Action = AuditAction.Updated,
                ResourceType = "Seat",
                ResourceId = seat.Id.ToString(),
                Details = $"Seat {seat.Id} updated",
                UserId = 0
            }, ct);

            await _uow.CommitTransactionAsync(ct);

            // Invalidate cache
            await _cacheService.RemoveByPrefixAsync("Seats:List", ct);
        }
        catch
        {
            await _uow.RollbackTransactionAsync(ct);
            throw;
        }
    }
}
