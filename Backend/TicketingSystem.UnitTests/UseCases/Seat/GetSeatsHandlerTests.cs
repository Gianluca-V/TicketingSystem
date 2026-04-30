using FluentAssertions;
using Moq;
using TicketingSystem.Application.DTOs;
using TicketingSystem.Application.Interfaces.persistence;
using TicketingSystem.Application.Interfaces.Services;
using TicketingSystem.Application.UseCases.Seat.GetSeats;
using TicketingSystem.Application.UseCases.Seat.Handlers;
using TicketingSystem.UnitTests.Helpers;
using TicketingSystem.Domain.Entities;
using TicketingSystem.Domain.QueryFilters;
using Xunit;

namespace TicketingSystem.UnitTests.UseCases.Seat;

public class GetSeatsHandlerTests
{
    private readonly Mock<ISeatRepository> _seatRepositoryMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly GetSeatsHandler _handler;

    public GetSeatsHandlerTests()
    {
        _seatRepositoryMock = new Mock<ISeatRepository>();
        _cacheServiceMock = MockHelpers.MockCacheService();
        _handler = new GetSeatsHandler(_seatRepositoryMock.Object, _cacheServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WhenSeatsExist_ShouldReturnDtos()
    {
        // Arrange
        var seats = new List<TicketingSystem.Domain.Entities.Seat>
        {
            new() { Id = 1, SeatNumber = "A1", Status = SeatStatus.Available }
        };

        _cacheServiceMock.Setup(c => c.GetAsync<IEnumerable<SeatDto>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IEnumerable<SeatDto>?)null);

        _seatRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<SeatFilter>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(seats);

        // Act
        var result = await _handler.Handle(new GetSeatsQuery(), CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result.First().SeatNumber.Should().Be("A1");
    }
}
