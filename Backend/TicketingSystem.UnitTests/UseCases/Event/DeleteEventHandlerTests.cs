using FluentAssertions;
using Moq;
using TicketingSystem.Application.Interfaces.persistence;
using TicketingSystem.Application.Interfaces.Services;
using TicketingSystem.Application.UseCases.Event.DeleteEvent;
using TicketingSystem.Domain.Entities;
using Xunit;

namespace TicketingSystem.UnitTests.UseCases.Event;

public class DeleteEventHandlerTests
{
    private readonly Mock<IEventRepository> _eventRepositoryMock;
    private readonly Mock<IAuditRepository> _auditRepositoryMock;
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly DeleteEventHandler _handler;

    public DeleteEventHandlerTests()
    {
        _eventRepositoryMock = new Mock<IEventRepository>();
        _auditRepositoryMock = new Mock<IAuditRepository>();
        _uowMock = new Mock<IUnitOfWork>();

        _handler = new DeleteEventHandler(
            _eventRepositoryMock.Object,
            _auditRepositoryMock.Object,
            _uowMock.Object
        );
    }

    [Fact]
    public async Task Handle_ValidId_ShouldDeleteEvent()
    {
        // Arrange
        var command = new DeleteEventCommand { Id = 1 };
        var @event = new TicketingSystem.Domain.Entities.Event { Id = 1, Name = "E1", Status = "A" };

        _eventRepositoryMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(@event);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _eventRepositoryMock.Verify(r => r.DeleteAsync(1, It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
