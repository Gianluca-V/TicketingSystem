using FluentAssertions;
using Moq;
using TicketingSystem.Application.Interfaces.persistence;
using TicketingSystem.Application.Interfaces.Services;
using TicketingSystem.Application.UseCases.Event.GetEvents;
using TicketingSystem.UnitTests.Helpers;
using Xunit;

namespace TicketingSystem.UnitTests.UseCases.Event;

public class GetEventByIdHandlerTests
{
    private readonly Mock<IEventRepository> _eventRepositoryMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly GetEventByIdHandler _handler;

    public GetEventByIdHandlerTests()
    {
        _eventRepositoryMock = new Mock<IEventRepository>();
        _cacheServiceMock = MockHelpers.MockCacheService();
        _handler = new GetEventByIdHandler(_eventRepositoryMock.Object, _cacheServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WhenEventExists_ShouldReturnDto()
    {
        // Arrange
        var entity = new TicketingSystem.Domain.Entities.Event
        {
            Id = 10,
            Name = "Rock Festival",
            Venue = "Stadium",
            Status = "Active",
            EventDate = new DateTime(2026, 05, 10)
        };

        _eventRepositoryMock
            .Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        var query = new GetEventByIdQuery
        {
            Id = 10
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(10);
        result.Name.Should().Be("Rock Festival");
        result.Venue.Should().Be("Stadium");
        result.Date.Should().Be(new DateTime(2026, 05, 10));
    }

    [Fact]
    public async Task Handle_WhenEventDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        _eventRepositoryMock
            .Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TicketingSystem.Domain.Entities.Event?)null);

        var query = new GetEventByIdQuery
        {
            Id = 99
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }
}