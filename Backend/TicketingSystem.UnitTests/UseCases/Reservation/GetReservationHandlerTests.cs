using FluentAssertions;
using Moq;
using TicketingSystem.Application.DTOs;
using TicketingSystem.Application.Interfaces.persistence;
using TicketingSystem.Application.Interfaces.Services;
using TicketingSystem.Application.UseCases.Reservation.GetReservation;
using TicketingSystem.Domain.Entities;
using TicketingSystem.Domain.QueryFilters;
using TicketingSystem.UnitTests.Helpers;
using Xunit;

namespace TicketingSystem.UnitTests.UseCases.Reservation;

public class GetReservationHandlerTests
{
    private readonly Mock<IReservationRepository> _reservationRepositoryMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly GetReservationByIdHandler _handler;

    public GetReservationHandlerTests()
    {
        _reservationRepositoryMock = new Mock<IReservationRepository>();
        _cacheServiceMock = MockHelpers.MockCacheService();
        _handler = new GetReservationByIdHandler(_reservationRepositoryMock.Object, _cacheServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WhenCacheExists_ShouldReturnCachedReservation()
    {
        // Arrange
        var id = Guid.NewGuid();
        var cachedReservation = new ReservationDto(id, 1, "10", DateTime.UtcNow, DateTime.UtcNow.AddMinutes(10), false);
        _cacheServiceMock.Setup(c => c.GetAsync<ReservationDto>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedReservation);

        // Act
        var result = await _handler.Handle(new GetReservationByIdQuery(id), CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(cachedReservation);
        _reservationRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenCacheMiss_ShouldFetchAndCache()
    {
        // Arrange
        var id = Guid.NewGuid();
        var reservation = new TicketingSystem.Domain.Entities.Reservation { Id = id, SeatId = 1, UserId = 10 };
        _cacheServiceMock.Setup(c => c.GetAsync<ReservationDto>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ReservationDto?)null);
        _reservationRepositoryMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservation);

        // Act
        var result = await _handler.Handle(new GetReservationByIdQuery(id), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        _cacheServiceMock.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<ReservationDto>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenReservationExists_ShouldReturnDto()
    {
        // Arrange
        var id = Guid.NewGuid();
        var reservation = new TicketingSystem.Domain.Entities.Reservation { Id = id, SeatId = 1, UserId = 10 };

        _cacheServiceMock.Setup(c => c.GetAsync<ReservationDto>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ReservationDto?)null);

        _reservationRepositoryMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservation);

        // Act
        var result = await _handler.Handle(new GetReservationByIdQuery(id), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(id);
    }
}

public class GetReservationsHandlerTests
{
    private readonly Mock<IReservationRepository> _reservationRepositoryMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly GetReservationsHandler _handler;

    public GetReservationsHandlerTests()
    {
        _reservationRepositoryMock = new Mock<IReservationRepository>();
        _cacheServiceMock = MockHelpers.MockCacheService();
        _handler = new GetReservationsHandler(_reservationRepositoryMock.Object, _cacheServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WhenCacheExists_ShouldReturnCachedData()
    {
        // Arrange
        var query = new GetReservationsQuery();
        var cachedData = new List<ReservationDto> { new(Guid.NewGuid(), 1, "10", DateTime.UtcNow, DateTime.UtcNow, false) };
        _cacheServiceMock.Setup(c => c.GetAsync<IEnumerable<ReservationDto>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedData);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(cachedData);
    }

    [Fact]
    public async Task Handle_WhenCacheMiss_ShouldFetchAndCache()
    {
        // Arrange
        var query = new GetReservationsQuery();
        var reservations = new List<TicketingSystem.Domain.Entities.Reservation>
        {
            new() { Id = Guid.NewGuid(), SeatId = 1, UserId = 10 }
        };
        _cacheServiceMock.Setup(c => c.GetAsync<IEnumerable<ReservationDto>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IEnumerable<ReservationDto>?)null);
        _reservationRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<ReservationFilter>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservations);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        _cacheServiceMock.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<IEnumerable<ReservationDto>>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
