using FluentAssertions;
using Moq;
using TicketingSystem.Application.Interfaces.persistence;
using TicketingSystem.Application.Interfaces.Services;
using TicketingSystem.Application.UseCases.Event.UpdateEvent;
using TicketingSystem.Domain.Entities;
using TicketingSystem.UnitTests.Helpers;
using Xunit;

namespace TicketingSystem.UnitTests.UseCases.Event;

public class UpdateEventHandlerTests
{
    private readonly Mock<IEventRepository> _eventRepositoryMock;
    private readonly Mock<IAuditRepository> _auditRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly UpdateEventHandler _handler;

    public UpdateEventHandlerTests()
    {
        _eventRepositoryMock = new Mock<IEventRepository>();
        _auditRepositoryMock = new Mock<IAuditRepository>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _cacheServiceMock = MockHelpers.MockCacheService();
        _uowMock = MockHelpers.MockUnitOfWork();

        _handler = new UpdateEventHandler(
            _eventRepositoryMock.Object,
            _auditRepositoryMock.Object,
            _currentUserServiceMock.Object,
            _cacheServiceMock.Object,
            _uowMock.Object
        );
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldUpdateEvent()
    {
        // Arrange
        var command = new UpdateEventCommand 
        { 
            Id = 1, 
            Name = "Updated Festival",
            Date = DateTime.Now,
            Venue = "New Venue",
            Status = "Inactive"
        };
        var @event = new TicketingSystem.Domain.Entities.Event { Id = 1, Name = "Old Festival", Status = "A" };

        _eventRepositoryMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(@event);
        _currentUserServiceMock.Setup(s => s.UserId).Returns(456);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        @event.Name.Should().Be("Updated Festival");
        @event.EventDate.Kind.Should().Be(DateTimeKind.Utc);
        @event.Venue.Should().Be("New Venue");
        @event.Status.Should().Be("Inactive");
        
        _eventRepositoryMock.Verify(r => r.UpdateAsync(@event, It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        
        _auditRepositoryMock.Verify(a => a.AddAsync(It.Is<AuditLog>(l => 
            l.Action == AuditAction.Updated && 
            l.UserId == 456), It.IsAny<CancellationToken>()), Times.Once);

        _cacheServiceMock.Verify(c => c.RemoveAsync($"Event:{command.Id}", It.IsAny<CancellationToken>()), Times.Once);
        _cacheServiceMock.Verify(c => c.RemoveByPrefixAsync("Events:List", It.IsAny<CancellationToken>()), Times.Once);
        _cacheServiceMock.Verify(c => c.RemoveByPrefixAsync("AuditLogs:List", It.IsAny<CancellationToken>()), Times.Once);
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
        _uowMock.Verify(u => u.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_GenericException_ShouldRollback()
    {
        // Arrange
        var command = new UpdateEventCommand { Id = 1, Name = "Error" };
        var @event = new TicketingSystem.Domain.Entities.Event { Id = 1, Name = "E1", Status = "A" };
        _eventRepositoryMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(@event);
        _eventRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<TicketingSystem.Domain.Entities.Event>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>();
        _uowMock.Verify(u => u.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
