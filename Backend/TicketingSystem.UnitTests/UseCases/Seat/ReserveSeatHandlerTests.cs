using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using TicketingSystem.Application.DTOs;
using TicketingSystem.Application.Interfaces.persistence;
using TicketingSystem.Application.Interfaces.Services;
using TicketingSystem.Application.UseCases.Seat.ReserveSeat;
using TicketingSystem.Domain.Entities;
using Xunit;

namespace TicketingSystem.UnitTests.UseCases.Seat;

public class ReserveSeatHandlerTests
{
    private readonly Mock<ISeatRepository> _seatRepositoryMock;
    private readonly Mock<IReservationRepository> _reservationRepositoryMock;
    private readonly Mock<IAuditRepository> _auditRepositoryMock;
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly ReserveSeatHandler _handler;

    public ReserveSeatHandlerTests()
    {
        _seatRepositoryMock = new Mock<ISeatRepository>();
        _reservationRepositoryMock = new Mock<IReservationRepository>();
        _auditRepositoryMock = new Mock<IAuditRepository>();
        _uowMock = new Mock<IUnitOfWork>();

        _handler = new ReserveSeatHandler(
            _seatRepositoryMock.Object,
            _reservationRepositoryMock.Object,
            _auditRepositoryMock.Object,
            _uowMock.Object
        );
    }

    [Fact]
    public async Task Handle_AvailableSeat_ShouldCreateReservation()
    {
        // Arrange
        var command = new ReserveSeatCommand(1, 10);
        var seat = new TicketingSystem.Domain.Entities.Seat { Id = 1, SeatNumber = "A1", Status = SeatStatus.Available };

        _seatRepositoryMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(seat);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        seat.Status.Should().Be(SeatStatus.Reserved);
        _reservationRepositoryMock.Verify(r => r.AddAsync(It.IsAny<TicketingSystem.Domain.Entities.Reservation>(), It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_AlreadyReservedSeat_ShouldThrowException()
    {
        // Arrange
        var command = new ReserveSeatCommand(1, 10);
        var seat = new TicketingSystem.Domain.Entities.Seat { Id = 1, SeatNumber = "A1", Status = SeatStatus.Reserved };

        _seatRepositoryMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(seat);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Seat not available");
        _uowMock.Verify(u => u.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ConcurrencyConflict_ShouldThrowSeatAlreadyTaken()
    {
        // Arrange
        var command = new ReserveSeatCommand(1, 10);
        var seat = new TicketingSystem.Domain.Entities.Seat { Id = 1, SeatNumber = "A1", Status = SeatStatus.Available };

        _seatRepositoryMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(seat);
        
        _seatRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<TicketingSystem.Domain.Entities.Seat>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DbUpdateConcurrencyException());

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Seat already taken");
        _uowMock.Verify(u => u.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
