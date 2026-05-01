using TicketingSystem.Application.Interfaces.persistence;
using TicketingSystem.Application.Interfaces.Services;
using TicketingSystem.Domain.Entities;
using TicketingSystem.Domain.Exceptions;

namespace TicketingSystem.Application.UseCases.Seat.CreateSeat;

public class CreateSeatsBulkHandler : ICommandHandler<CreateSeatsBulkCommand, IEnumerable<int>>
{
    private readonly ISeatRepository _seatRepository;
    private readonly ISectorRepository _sectorRepository;
    private readonly IAuditRepository _auditRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICacheService _cacheService;
    private readonly IUnitOfWork _uow;

    public CreateSeatsBulkHandler(
        ISeatRepository seatRepository, 
        ISectorRepository sectorRepository, 
        IAuditRepository auditRepository, 
        ICurrentUserService currentUserService,
        ICacheService cacheService,
        IUnitOfWork uow)
    {
        _seatRepository = seatRepository;
        _sectorRepository = sectorRepository;
        _auditRepository = auditRepository;
        _currentUserService = currentUserService;
        _cacheService = cacheService;
        _uow = uow;
    }

    public async Task<IEnumerable<int>> Handle(CreateSeatsBulkCommand command, CancellationToken ct)
    {
        await _uow.BeginTransactionAsync(ct);
        try
        {
            var sector = await _sectorRepository.GetByIdAsync(command.SectorId, ct);
            if (sector == null) throw new Exception("Sector not found");

            // Capacity validation
            var currentSeatsCount = await _seatRepository.CountBySectorAsync(command.SectorId, ct);
            if (currentSeatsCount + command.Seats.Count > sector.Capacity)
            {
                throw new BusinessException($"Cannot create {command.Seats.Count} seats. Sector capacity is {sector.Capacity} and it already has {currentSeatsCount} seats.");
            }

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

            // Invalidate cache
            await _cacheService.RemoveByPrefixAsync("Seats:List", ct);

            return seats.Select(s => s.Id);
        }
        catch
        {
            await _uow.RollbackTransactionAsync(ct);
            throw;
        }
    }
}
