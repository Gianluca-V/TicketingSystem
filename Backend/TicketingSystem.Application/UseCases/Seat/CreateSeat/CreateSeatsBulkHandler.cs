using TicketingSystem.Application.Interfaces.persistence;
using TicketingSystem.Application.Interfaces.Services;
using TicketingSystem.Domain.Entities;

namespace TicketingSystem.Application.UseCases.Seat.CreateSeat;

public class CreateSeatsBulkHandler : ICommandHandler<CreateSeatsBulkCommand, IEnumerable<int>>
{
    private readonly ISeatRepository _seatRepository;
    private readonly ISectorRepository _sectorRepository;
    private readonly IAuditRepository _auditRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _uow;

    public CreateSeatsBulkHandler(
        ISeatRepository seatRepository, 
        ISectorRepository sectorRepository, 
        IAuditRepository auditRepository, 
        ICurrentUserService currentUserService,
        IUnitOfWork uow)
    {
        _seatRepository = seatRepository;
        _sectorRepository = sectorRepository;
        _auditRepository = auditRepository;
        _currentUserService = currentUserService;
        _uow = uow;
    }

    public async Task<IEnumerable<int>> Handle(CreateSeatsBulkCommand command, CancellationToken ct)
    {
        await _uow.BeginTransactionAsync(ct);
        try
        {
            var sector = await _sectorRepository.GetByIdAsync(command.SectorId, ct);
            if (sector == null) throw new Exception("Sector not found");

            var seats = command.Seats.Select(s => new TicketingSystem.Domain.Entities.Seat
            {
                SectorId = command.SectorId,
                SeatNumber = s.SeatNumber,
                Price = s.Price,
                RowIdentifier = s.RowIdentifier,
                Status = SeatStatus.Available
            }).ToList();

            await _seatRepository.AddBulkAsync(seats, ct);
            await _uow.SaveChangesAsync(ct);

            await _auditRepository.AddAsync(new AuditLog
            {
                Action = AuditAction.Created,
                ResourceType = "Seat",
                ResourceId = "Bulk",
                Details = $"{seats.Count} seats created for Sector {command.SectorId}",
                UserId = _currentUserService.UserId ?? 0
            }, ct);

            await _uow.CommitTransactionAsync(ct);
            return seats.Select(s => s.Id);
        }
        catch
        {
            await _uow.RollbackTransactionAsync(ct);
            throw;
        }
    }
}
