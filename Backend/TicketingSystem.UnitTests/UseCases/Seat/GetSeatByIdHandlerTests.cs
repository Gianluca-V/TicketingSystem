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
    public async Task Handle_WhenCacheExists_ShouldReturnCachedSeat()
    {
        // Arrange
        var cachedSeat = new SeatDto(10, "A1", 5, "S1", 1500, "Available");
        var query = new GetSeatByIdQuery { SeatId = 10, SectorId = 5 };

        _cacheServiceMock.Setup(c => c.GetAsync<SeatDto>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedSeat);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(cachedSeat);
        _seatRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenCacheMiss_ShouldFetchAndCache()
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

        _cacheServiceMock.Setup(c => c.GetAsync<SeatDto>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((SeatDto?)null);

        _seatRepositoryMock.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(seat);

        var query = new GetSeatByIdQuery { SeatId = 10, SectorId = 5 };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        _cacheServiceMock.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<SeatDto>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Once);
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