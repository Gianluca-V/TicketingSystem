using TicketingSystem.Application.Interfaces.persistence;
using TicketingSystem.Application.Interfaces.Services;
using TicketingSystem.Domain.Entities;

namespace TicketingSystem.Application.UseCases.Seat.CreateSeat;

public class CreateSeatHandler : ICommandHandler<CreateSeatCommand, int>
{
    private readonly ISeatRepository _seatRepository;
    private readonly ISectorRepository _sectorRepository;
    private readonly IAuditRepository _auditRepository;
    private readonly ICacheService _cacheService;
    private readonly IUnitOfWork _uow;

    public CreateSeatHandler(ISeatRepository seatRepository, ISectorRepository sectorRepository, IAuditRepository auditRepository, ICacheService cacheService, IUnitOfWork uow)
    {
        _seatRepository = seatRepository;
        _sectorRepository = sectorRepository;
        _auditRepository = auditRepository;
        _cacheService = cacheService;
        _uow = uow;
    }

    public async Task<int> Handle(CreateSeatCommand command, CancellationToken ct)
    {
        await _uow.BeginTransactionAsync(ct);
        try
        {
            var sector = await _sectorRepository.GetByIdAsync(command.SectorId, ct);
            if (sector == null) throw new Exception("Sector not found");

            var seat = new TicketingSystem.Domain.Entities.Seat
            {
                SectorId = command.SectorId,
                SeatNumber = command.SeatNumber,
                Price = command.Price,
                RowIdentifier = command.RowIdentifier,
                Status = SeatStatus.Available
            };

            await _seatRepository.AddAsync(seat, ct);
            await _uow.SaveChangesAsync(ct);

            await _auditRepository.AddAsync(new AuditLog
            {
                Action = AuditAction.Created,
                ResourceType = "Seat",
                ResourceId = seat.Id.ToString(),
                Details = $"Seat {seat.SeatNumber} created for Sector {command.SectorId}",
                UserId = 0
            }, ct);

            await _uow.CommitTransactionAsync(ct);

            // Invalidate cache
            await _cacheService.RemoveByPrefixAsync("Seats:List", ct);

            return seat.Id;
        }
        catch
        {
            await _uow.RollbackTransactionAsync(ct);
            throw;
        }
    }
}
