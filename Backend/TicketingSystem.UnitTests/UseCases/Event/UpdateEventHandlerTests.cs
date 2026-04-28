using FluentAssertions;
using Moq;
using TicketingSystem.Application.Interfaces.persistence;
using TicketingSystem.Application.Interfaces.Services;
using TicketingSystem.Application.UseCases.Event.UpdateEvent;
using TicketingSystem.Domain.Entities;
using Xunit;

namespace TicketingSystem.UnitTests.UseCases.Event;

public class UpdateEventHandlerTests
{
    private readonly Mock<IEventRepository> _eventRepositoryMock;
    private readonly Mock<IAuditRepository> _auditRepositoryMock;
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly UpdateEventHandler _handler;

    public UpdateEventHandlerTests()
    {
        _eventRepositoryMock = new Mock<IEventRepository>();
        _auditRepositoryMock = new Mock<IAuditRepository>();
        _uowMock = new Mock<IUnitOfWork>();

        _handler = new UpdateEventHandler(
            _eventRepositoryMock.Object,
            _auditRepositoryMock.Object,
            _uowMock.Object
        );
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldUpdateEvent()
    {
        // Arrange
        var command = new UpdateEventCommand { Id = 1, Name = "Updated Festival" };
        var @event = new TicketingSystem.Domain.Entities.Event { Id = 1, Name = "Old Festival" };

        _eventRepositoryMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(@event);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        @event.Name.Should().Be("Updated Festival");
        _eventRepositoryMock.Verify(r => r.UpdateAsync(@event, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_UpdateEventDate_ShouldUpdateEventDate()
    {
        // Arrange
        var newDate = DateTime.Now.AddDays(10);
        var command = new UpdateEventCommand { Id = 1, Date = newDate };
        var @event = new TicketingSystem.Domain.Entities.Event { Id = 1, Name = "Festival", EventDate = DateTime.Now };

        _eventRepositoryMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(@event);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        @event.EventDate.Should().Be(newDate);
        _eventRepositoryMock.Verify(r => r.UpdateAsync(@event, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_EventNotFound_ShouldThrowException()
    {
        // Arrange
        var command = new UpdateEventCommand { Id = 99 };
        _eventRepositoryMock.Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TicketingSystem.Domain.Entities.Event?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Event not found");
    }
}
