using FluentAssertions;
using Moq;
using TicketingSystem.Application.Interfaces.persistence;
using TicketingSystem.Application.Interfaces.Services;
using TicketingSystem.Application.UseCases.Seat.UpdateSeat;
using TicketingSystem.Domain.Entities;
using Xunit;

namespace TicketingSystem.UnitTests.UseCases.Seat;

public class UpdateSeatHandlerTests
{
    private readonly Mock<ISeatRepository> _seatRepositoryMock;
    private readonly Mock<IAuditRepository> _auditRepositoryMock;
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly UpdateSeatHandler _handler;

    public UpdateSeatHandlerTests()
    {
        _seatRepositoryMock = new Mock<ISeatRepository>();
        _auditRepositoryMock = new Mock<IAuditRepository>();
        _uowMock = new Mock<IUnitOfWork>();

        _handler = new UpdateSeatHandler(
            _seatRepositoryMock.Object,
            _auditRepositoryMock.Object,
            _uowMock.Object
        );
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldUpdateSeat()
    {
        // Arrange
        var command = new UpdateSeatCommand { Id = 1, SeatNumber = "B2" };
        var seat = new TicketingSystem.Domain.Entities.Seat { Id = 1, SeatNumber = "A1" };

        _seatRepositoryMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(seat);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        seat.SeatNumber.Should().Be("B2");
        _seatRepositoryMock.Verify(r => r.UpdateAsync(seat, It.IsAny<CancellationToken>()), Times.Once);
    }
}
