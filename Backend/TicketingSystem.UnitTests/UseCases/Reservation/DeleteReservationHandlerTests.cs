using FluentAssertions;
using Moq;
using TicketingSystem.Application.Interfaces.persistence;
using TicketingSystem.Application.Interfaces.Services;
using TicketingSystem.Application.UseCases.Reservation.DeleteReservation;
using TicketingSystem.Domain.Entities;
using TicketingSystem.UnitTests.Helpers;
using Xunit;

namespace TicketingSystem.UnitTests.UseCases.Reservation;

public class DeleteReservationHandlerTests
{
    private readonly Mock<IReservationRepository> _reservationRepositoryMock;
    private readonly Mock<ISeatRepository> _seatRepositoryMock;
    private readonly Mock<IAuditRepository> _auditRepositoryMock;
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly DeleteReservationHandler _handler;

    public DeleteReservationHandlerTests()
    {
        _reservationRepositoryMock = new Mock<IReservationRepository>();
        _seatRepositoryMock = new Mock<ISeatRepository>();
        _auditRepositoryMock = new Mock<IAuditRepository>();
        _uowMock = MockHelpers.MockUnitOfWork();
        _cacheServiceMock = MockHelpers.MockCacheService();

        _handler = new DeleteReservationHandler(
            _reservationRepositoryMock.Object,
            _seatRepositoryMock.Object,
            _auditRepositoryMock.Object,
            _uowMock.Object,
            _cacheServiceMock.Object
        );
    }

    [Fact]
    public async Task Handle_ExistingReservation_ShouldDeleteAndReleaseSeat()
    {
        // Arrange
        var reservationId = Guid.NewGuid();
        var command = new DeleteReservationCommand(reservationId);
        var reservation = new TicketingSystem.Domain.Entities.Reservation { Id = reservationId, SeatId = 1, UserId = 10 };
        var seat = new TicketingSystem.Domain.Entities.Seat { Id = 1, Status = SeatStatus.Reserved, SeatNumber = "A1" };

        _reservationRepositoryMock.Setup(r => r.GetByIdAsync(reservationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservation);
        _seatRepositoryMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(seat);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        seat.Status.Should().Be(SeatStatus.Available);
        _reservationRepositoryMock.Verify(r => r.DeleteAsync(reservation, It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _auditRepositoryMock.Verify(a => a.AddAsync(It.Is<AuditLog>(l => l.Action == AuditAction.Released), It.IsAny<CancellationToken>()), Times.Once);
        _cacheServiceMock.Verify(c => c.RemoveByPrefixAsync("Reservations:List", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NonExistingReservation_ShouldReturnImmediately()
    {
        // Arrange
        var reservationId = Guid.NewGuid();
        var command = new DeleteReservationCommand(reservationId);
        _reservationRepositoryMock.Setup(r => r.GetByIdAsync(reservationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TicketingSystem.Domain.Entities.Reservation?)null);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _reservationRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<TicketingSystem.Domain.Entities.Reservation>(), It.IsAny<CancellationToken>()), Times.Never);
        _uowMock.Verify(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_GenericException_ShouldRollback()
    {
        // Arrange
        var reservationId = Guid.NewGuid();
        var command = new DeleteReservationCommand(reservationId);
        _reservationRepositoryMock.Setup(r => r.GetByIdAsync(reservationId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>();
        _uowMock.Verify(u => u.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
