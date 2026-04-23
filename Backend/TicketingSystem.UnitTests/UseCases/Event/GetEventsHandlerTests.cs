using FluentAssertions;
using Moq;
using TicketingSystem.Application.DTOs;
using TicketingSystem.Application.Interfaces.persistence;
using TicketingSystem.Application.Interfaces.Services;
using TicketingSystem.Application.UseCases.Event.GetEvents;
using TicketingSystem.Domain.Entities;
using TicketingSystem.Domain.QueryFilters;
using Xunit;

namespace TicketingSystem.UnitTests.UseCases.Event;

public class GetEventsHandlerTests
{
    private readonly Mock<IEventRepository> _eventRepositoryMock;
    private readonly GetEventsHandler _handler;

    public GetEventsHandlerTests()
    {
        _eventRepositoryMock = new Mock<IEventRepository>();
        _handler = new GetEventsHandler(_eventRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WhenEventsExist_ShouldReturnEventDtos()
    {
        // Arrange
        var events = new List<TicketingSystem.Domain.Entities.Event>
        {
            new() { Id = 1, Name = "E1", Venue = "V1", Status = "A" },
            new() { Id = 2, Name = "E2", Venue = "V2", Status = "A" }
        };

        _eventRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<EventFilter>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(events);

        // Act
        var result = await _handler.Handle(new GetEventsQuery(), CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.First().Name.Should().Be("E1");
    }
}
