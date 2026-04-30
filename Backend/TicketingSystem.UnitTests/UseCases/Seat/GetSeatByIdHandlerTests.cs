using FluentAssertions;
using Moq;
using TicketingSystem.Application.DTOs;
using TicketingSystem.Application.Interfaces.persistence;
using TicketingSystem.Application.Interfaces.Services;
using TicketingSystem.Application.UseCases.Seat.GetSeats;
using TicketingSystem.UnitTests.Helpers;
using TicketingSystem.Domain.Entities;
using TicketingSystem.Domain.QueryFilters;
using Xunit;

namespace TicketingSystem.UnitTests.UseCases.Seat;

public class GetSeatByIdHandlerTests
{
    private readonly Mock<ISeatRepository> _seatRepositoryMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly GetSeatByIdHandler _handler;

    public GetSeatByIdHandlerTests()
    {
        _seatRepositoryMock = new Mock<ISeatRepository>();
        _cacheServiceMock = MockHelpers.MockCacheService();
        _handler = new GetSeatByIdHandler(_seatRepositoryMock.Object, _cacheServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WhenSeatExistsAndBelongsToSector_ShouldReturnDto()
    {
        // Arrange
        var seat = new TicketingSystem.Domain.Entities.Seat
        {
            Id = 10,
            SeatNumber = "A1",
            SectorId = 5,
            Price = 1500,
            Status = SeatStatus.Available
        };

        _seatRepositoryMock
            .Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(seat);

        var query = new GetSeatByIdQuery
        {
            EventId = 1,
            SectorId = 5,
            SeatId = 10
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(10);
        result.SeatNumber.Should().Be("A1");
        result.SectorId.Should().Be(5);
        result.Status.Should().Be("Available");
    }

    [Fact]
    public async Task Handle_WhenSeatDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        _seatRepositoryMock
            .Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TicketingSystem.Domain.Entities.Seat?)null);

        var query = new GetSeatByIdQuery
        {
            EventId = 1,
            SectorId = 5,
            SeatId = 99
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenSeatBelongsToAnotherSector_ShouldReturnNull()
    {
        // Arrange
        var seat = new TicketingSystem.Domain.Entities.Seat
        {
            Id = 10,
            SeatNumber = "A1",
            SectorId = 99,
            Price = 1500,
            Status = SeatStatus.Available
        };

        _seatRepositoryMock
            .Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(seat);

        var query = new GetSeatByIdQuery
        {
            EventId = 1,
            SectorId = 5,
            SeatId = 10
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }
}