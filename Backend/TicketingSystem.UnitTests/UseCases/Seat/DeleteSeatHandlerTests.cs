using FluentAssertions;
using Moq;
using TicketingSystem.Application.Interfaces.persistence;
using TicketingSystem.Application.Interfaces.Services;
using TicketingSystem.Application.UseCases.Seat.DeleteSeat;
using TicketingSystem.Domain.Entities;
using TicketingSystem.UnitTests.Helpers;
using Xunit;

namespace TicketingSystem.UnitTests.UseCases.Seat;

public class DeleteSeatHandlerTests
{
    private readonly Mock<ISeatRepository> _seatRepositoryMock;
    private readonly Mock<IAuditRepository> _auditRepositoryMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly DeleteSeatHandler _handler;

    public DeleteSeatHandlerTests()
    {
        _seatRepositoryMock = new Mock<ISeatRepository>();
        _auditRepositoryMock = new Mock<IAuditRepository>();
        _cacheServiceMock = MockHelpers.MockCacheService();
        _uowMock = MockHelpers.MockUnitOfWork();

        _handler = new DeleteSeatHandler(
            _seatRepositoryMock.Object,
            _auditRepositoryMock.Object,
            _cacheServiceMock.Object,
            _uowMock.Object
        );
    }

    [Fact]
    public async Task Handle_ValidId_ShouldDeleteSeat()
    {
        // Arrange
        var command = new DeleteSeatCommand { Id = 1 };
        var seat = new TicketingSystem.Domain.Entities.Seat { Id = 1, SeatNumber = "A1", Status = SeatStatus.Available };

        _seatRepositoryMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(seat);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _seatRepositoryMock.Verify(r => r.DeleteAsync(1, It.IsAny<CancellationToken>()), Times.Once);
    }
}
