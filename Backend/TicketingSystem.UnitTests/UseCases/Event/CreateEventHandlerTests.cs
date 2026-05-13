using FluentAssertions;
using Moq;
using TicketingSystem.Application.Interfaces.persistence;
using TicketingSystem.Application.Interfaces.Services;
using TicketingSystem.Application.UseCases.Event.CreateEvent;
using TicketingSystem.Domain.Entities;
using TicketingSystem.UnitTests.Helpers;
using Xunit;

namespace TicketingSystem.UnitTests.UseCases.Event;

public class CreateEventHandlerTests
{
    private readonly Mock<IEventRepository> _eventRepositoryMock;
    private readonly Mock<IAuditRepository> _auditRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly CreateEventHandler _handler;

    public CreateEventHandlerTests()
    {
        _eventRepositoryMock = new Mock<IEventRepository>();
        _auditRepositoryMock = new Mock<IAuditRepository>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _cacheServiceMock = MockHelpers.MockCacheService();
        _uowMock = MockHelpers.MockUnitOfWork();

        _handler = new CreateEventHandler(
            _eventRepositoryMock.Object,
            _auditRepositoryMock.Object,
            _currentUserServiceMock.Object,
            _cacheServiceMock.Object,
            _uowMock.Object
        );
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateEvent()
    {
        // Arrange
        var command = new CreateEventCommand
        {
            Name = "Festival",
            Date = DateTime.Now, // Local time
            Venue = "Stadium",
            Status = "Active"
        };

        _currentUserServiceMock.Setup(s => s.UserId).Returns(123);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        _eventRepositoryMock.Verify(r => r.AddAsync(It.Is<TicketingSystem.Domain.Entities.Event>(e => 
            e.Name == command.Name && 
            e.EventDate.Kind == DateTimeKind.Utc &&
            e.Venue == command.Venue &&
            e.Status == command.Status), It.IsAny<CancellationToken>()), Times.Once);
        
        _uowMock.Verify(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _auditRepositoryMock.Verify(a => a.AddAsync(It.Is<AuditLog>(l => 
            l.Action == AuditAction.Created && 
            l.UserId == 123), It.IsAny<CancellationToken>()), Times.Once);
        
        _cacheServiceMock.Verify(c => c.RemoveByPrefixAsync("Events:List", It.IsAny<CancellationToken>()), Times.Once);
        _cacheServiceMock.Verify(c => c.RemoveByPrefixAsync("AuditLogs:List", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_GenericException_ShouldRollback()
    {
        // Arrange
        var command = new CreateEventCommand { Name = "Error" };
        _eventRepositoryMock.Setup(r => r.AddAsync(It.IsAny<TicketingSystem.Domain.Entities.Event>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>();
        _uowMock.Verify(u => u.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
