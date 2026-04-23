using FluentAssertions;
using Moq;
using TicketingSystem.Application.Interfaces.persistence;
using TicketingSystem.Application.Interfaces.Services;
using TicketingSystem.Application.UseCases.Seat.DeleteSeat;
using TicketingSystem.Domain.Entities;
using Xunit;

namespace TicketingSystem.UnitTests.UseCases.Seat;

public class DeleteSeatHandlerTests
{
    private readonly Mock<ISeatRepository> _seatRepositoryMock;
    private readonly Mock<IAuditRepository> _auditRepositoryMock;
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly DeleteSeatHandler _handler;

    public DeleteSeatHandlerTests()
    {
        _seatRepositoryMock = new Mock<ISeatRepository>();
        _auditRepositoryMock = new Mock<IAuditRepository>();
        _uowMock = new Mock<IUnitOfWork>();

        _handler = new DeleteSeatHandler(
            _seatRepositoryMock.Object,
            _auditRepositoryMock.Object,
            _uowMock.Object
        );
    }

    [Fact]
    public async Task Handle_ValidId_ShouldDeleteSeat()
    {
        // Arrange
        var command = new DeleteSeatCommand { Id = 1 };
        var seat = new TicketingSystem.Domain.Entities.Seat { Id = 1, SeatNumber = "A1" };

        _seatRepositoryMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(seat);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _seatRepositoryMock.Verify(r => r.DeleteAsync(1, It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
