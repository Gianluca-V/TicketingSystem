using FluentAssertions;
using Moq;
using TicketingSystem.Application.Interfaces.persistence;
using TicketingSystem.Application.Interfaces.Services;
using TicketingSystem.Application.UseCases.Seat.CreateSeat;
using TicketingSystem.Domain.Entities;
using TicketingSystem.UnitTests.Helpers;
using Xunit;

namespace TicketingSystem.UnitTests.UseCases.Seat;

public class CreateSeatsBulkHandlerTests
{
    private readonly Mock<ISeatRepository> _seatRepositoryMock;
    private readonly Mock<ISectorRepository> _sectorRepositoryMock;
    private readonly Mock<IAuditRepository> _auditRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly CreateSeatsBulkHandler _handler;

    public CreateSeatsBulkHandlerTests()
    {
        _seatRepositoryMock = new Mock<ISeatRepository>();
        _sectorRepositoryMock = new Mock<ISectorRepository>();
        _auditRepositoryMock = new Mock<IAuditRepository>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _cacheServiceMock = MockHelpers.MockCacheService();
        _uowMock = MockHelpers.MockUnitOfWork();

        _handler = new CreateSeatsBulkHandler(
            _seatRepositoryMock.Object,
            _sectorRepositoryMock.Object,
            _auditRepositoryMock.Object,
            _currentUserServiceMock.Object,
            _cacheServiceMock.Object,
            _uowMock.Object
        );
    }

    [Fact]
    public async Task Handle_SectorExists_ShouldCreateSeatsInBulk()
    {
        // Arrange
        var command = new CreateSeatsBulkCommand 
        { 
            SectorId = 1, 
            Seats = new List<SeatItem> 
            {
                new SeatItem { SeatNumber = "A1", Price = 10.0m },
                new SeatItem { SeatNumber = "A2", Price = 10.0m }
            }
        };
        var sector = new TicketingSystem.Domain.Entities.Sector 
        { 
            Id = 1, 
            Name = "S1",
            EventId = 1,
            Event = new TicketingSystem.Domain.Entities.Event { Name = "E1" }
        };

        _sectorRepositoryMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sector);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        _seatRepositoryMock.Verify(r => r.AddBulkAsync(It.IsAny<IEnumerable<TicketingSystem.Domain.Entities.Seat>>(), It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_SectorNotFound_ShouldThrowException()
    {
        // Arrange
        var command = new CreateSeatsBulkCommand { SectorId = 99 };
        _sectorRepositoryMock.Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TicketingSystem.Domain.Entities.Sector?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Sector not found");
        _uowMock.Verify(u => u.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
