using FluentAssertions;
using Moq;
using TicketingSystem.Application.DTOs;
using TicketingSystem.Application.Interfaces.persistence;
using TicketingSystem.Application.Interfaces.Services;
using TicketingSystem.Application.UseCases.Payment;
using TicketingSystem.Domain.Entities;
using Xunit;

namespace TicketingSystem.UnitTests.UseCases.Payment;

public class ProcessPaymentHandlerTests
{
    private readonly Mock<ISeatRepository> _seatRepositoryMock;
    private readonly Mock<IReservationRepository> _reservationRepositoryMock;
    private readonly Mock<IAuditRepository> _auditRepositoryMock;
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly ProcessPaymentHandler _handler;

    public ProcessPaymentHandlerTests()
    {
        _seatRepositoryMock = new Mock<ISeatRepository>();
        _reservationRepositoryMock = new Mock<IReservationRepository>();
        _auditRepositoryMock = new Mock<IAuditRepository>();
        _uowMock = new Mock<IUnitOfWork>();

        _handler = new ProcessPaymentHandler(
            _seatRepositoryMock.Object,
            _reservationRepositoryMock.Object,
            _auditRepositoryMock.Object,
            _uowMock.Object
        );
    }

    [Fact]
    public async Task Handle_ValidReservation_ShouldMarkAsSold()
    {
        // Arrange
        var reservationId = Guid.NewGuid();
        var command = new ProcessPaymentCommand(reservationId, "TX123");
        var reservation = new TicketingSystem.Domain.Entities.Reservation 
        { 
            Id = reservationId, 
            SeatId = 1, 
            UserId = 10,
            ReservedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10)
        };
        var seat = new TicketingSystem.Domain.Entities.Seat { Id = 1, SeatNumber = "A1", Status = SeatStatus.Reserved };

        _reservationRepositoryMock.Setup(r => r.GetByIdAsync(reservationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservation);
        _seatRepositoryMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(seat);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Status.Should().Be("Sold");
        seat.Status.Should().Be(SeatStatus.Sold);
        reservation.PaidAt.Should().NotBeNull();
        _uowMock.Verify(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ExpiredReservation_ShouldThrowException()
    {
        // Arrange
        var reservationId = Guid.NewGuid();
        var command = new ProcessPaymentCommand(reservationId, "TX123");
        var reservation = new TicketingSystem.Domain.Entities.Reservation 
        { 
            Id = reservationId, 
            ExpiresAt = DateTime.UtcNow.AddMinutes(-1) // Expired
        };

        _reservationRepositoryMock.Setup(r => r.GetByIdAsync(reservationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservation);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage($"Reservation {reservationId} has expired");
    }
}
