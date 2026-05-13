using FluentAssertions;
using Moq;
using TicketingSystem.Application.DTOs;
using TicketingSystem.Application.Interfaces.persistence;
using TicketingSystem.Application.Interfaces.Services;
using TicketingSystem.Application.UseCases.Payment;
using TicketingSystem.Domain.Entities;
using TicketingSystem.Domain.Exceptions;
using TicketingSystem.UnitTests.Helpers;
using Xunit;

namespace TicketingSystem.UnitTests.UseCases.Payment;

public class ProcessPaymentHandlerTests
{
    private readonly Mock<ISeatRepository> _seatRepositoryMock;
    private readonly Mock<IReservationRepository> _reservationRepositoryMock;
    private readonly Mock<IAuditRepository> _auditRepositoryMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly ProcessPaymentHandler _handler;

    public ProcessPaymentHandlerTests()
    {
        _seatRepositoryMock = new Mock<ISeatRepository>();
        _reservationRepositoryMock = new Mock<IReservationRepository>();
        _auditRepositoryMock = new Mock<IAuditRepository>();
        _cacheServiceMock = MockHelpers.MockCacheService();
        _uowMock = MockHelpers.MockUnitOfWork();

        _handler = new ProcessPaymentHandler(
            _seatRepositoryMock.Object,
            _reservationRepositoryMock.Object,
            _auditRepositoryMock.Object,
            _cacheServiceMock.Object,
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
        var seat = new TicketingSystem.Domain.Entities.Seat 
        { 
            Id = 1, 
            SeatNumber = "A1", 
            Status = SeatStatus.Reserved
        };

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
        _auditRepositoryMock.Verify(a => a.AddAsync(It.IsAny<AuditLog>(), It.IsAny<CancellationToken>()), Times.Once);
        _cacheServiceMock.Verify(c => c.RemoveByPrefixAsync("Reservations:List", It.IsAny<CancellationToken>()), Times.Once);
        _cacheServiceMock.Verify(c => c.RemoveByPrefixAsync("AuditLogs:List", It.IsAny<CancellationToken>()), Times.Once);
        _cacheServiceMock.Verify(c => c.RemoveByPrefixAsync("Seats:List", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ReservationNotFound_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var reservationId = Guid.NewGuid();
        var command = new ProcessPaymentCommand(reservationId, "TX123");
        _reservationRepositoryMock.Setup(r => r.GetByIdAsync(reservationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TicketingSystem.Domain.Entities.Reservation?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_AlreadyPaid_ShouldThrowConflictException()
    {
        // Arrange
        var reservationId = Guid.NewGuid();
        var command = new ProcessPaymentCommand(reservationId, "TX123");
        var reservation = new TicketingSystem.Domain.Entities.Reservation 
        { 
            Id = reservationId, 
            PaidAt = DateTime.UtcNow
        };
        _reservationRepositoryMock.Setup(r => r.GetByIdAsync(reservationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservation);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>().WithMessage("*already paid*");
    }

    [Fact]
    public async Task Handle_ExpiredReservation_ShouldThrowConflictException()
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
        await act.Should().ThrowAsync<ConflictException>().WithMessage($"Reservation {reservationId} has expired");
    }

    [Fact]
    public async Task Handle_SeatNotFound_ShouldThrowKeyNotFoundExceptionAndRollback()
    {
        // Arrange
        var reservationId = Guid.NewGuid();
        var command = new ProcessPaymentCommand(reservationId, "TX123");
        var reservation = new TicketingSystem.Domain.Entities.Reservation { Id = reservationId, SeatId = 99 };
        _reservationRepositoryMock.Setup(r => r.GetByIdAsync(reservationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservation);
        _seatRepositoryMock.Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TicketingSystem.Domain.Entities.Seat?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
        _uowMock.Verify(u => u.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_GenericException_ShouldRollback()
    {
        // Arrange
        var reservationId = Guid.NewGuid();
        var command = new ProcessPaymentCommand(reservationId, "TX123");
        var reservation = new TicketingSystem.Domain.Entities.Reservation { Id = reservationId, SeatId = 1 };
        _reservationRepositoryMock.Setup(r => r.GetByIdAsync(reservationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservation);
        _seatRepositoryMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>();
        _uowMock.Verify(u => u.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
