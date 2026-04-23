using FluentAssertions;
using Moq;
using TicketingSystem.Application.DTOs;
using TicketingSystem.Application.Interfaces.persistence;
using TicketingSystem.Application.UseCases.Reservation.GetReservation;
using TicketingSystem.Domain.Entities;
using Xunit;

namespace TicketingSystem.UnitTests.UseCases.Reservation;

public class GetReservationHandlerTests
{
    private readonly Mock<IReservationRepository> _reservationRepositoryMock;
    private readonly GetReservationHandler _handler;

    public GetReservationHandlerTests()
    {
        _reservationRepositoryMock = new Mock<IReservationRepository>();
        _handler = new GetReservationHandler(_reservationRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WhenReservationExists_ShouldReturnDto()
    {
        // Arrange
        var id = Guid.NewGuid();
        var reservation = new TicketingSystem.Domain.Entities.Reservation { Id = id, SeatId = 1, UserId = 10 };

        _reservationRepositoryMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservation);

        // Act
        var result = await _handler.Handle(new GetReservationQuery(id), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(id);
    }

    [Fact]
    public async Task Handle_WhenNotExists_ShouldReturnNull()
    {
        // Arrange
        var id = Guid.NewGuid();
        _reservationRepositoryMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TicketingSystem.Domain.Entities.Reservation?)null);

        // Act
        var result = await _handler.Handle(new GetReservationQuery(id), CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }
}
