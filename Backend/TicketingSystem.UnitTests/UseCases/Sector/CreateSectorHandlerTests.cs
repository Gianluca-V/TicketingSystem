using FluentAssertions;
using Moq;
using TicketingSystem.Application.Interfaces.persistence;
using TicketingSystem.Application.Interfaces.Services;
using TicketingSystem.Application.UseCases.Sector.CreateSector;
using TicketingSystem.Domain.Entities;
using Xunit;

namespace TicketingSystem.UnitTests.UseCases.Sector;

public class CreateSectorHandlerTests
{
    private readonly Mock<ISectorRepository> _sectorRepositoryMock;
    private readonly Mock<IEventRepository> _eventRepositoryMock;
    private readonly Mock<IAuditRepository> _auditRepositoryMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly CreateSectorHandler _handler;

    public CreateSectorHandlerTests()
    {
        _sectorRepositoryMock = new Mock<ISectorRepository>();
        _eventRepositoryMock = new Mock<IEventRepository>();
        _auditRepositoryMock = new Mock<IAuditRepository>();
        _cacheServiceMock = new Mock<ICacheService>();
        _uowMock = new Mock<IUnitOfWork>();

        _handler = new CreateSectorHandler(
            _sectorRepositoryMock.Object,
            _eventRepositoryMock.Object,
            _auditRepositoryMock.Object,
            _cacheServiceMock.Object,
            _uowMock.Object
        );
    }

    [Fact]
    public async Task Handle_EventExists_ShouldCreateSector()
    {
        // Arrange
        var command = new CreateSectorCommand { EventId = 1, Name = "VIP", Capacity = 100, Price = 50.0m };
        var @event = new TicketingSystem.Domain.Entities.Event { Id = 1, Name = "E1", Status = "A" };

        _eventRepositoryMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(@event);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeGreaterThanOrEqualTo(0);
        _sectorRepositoryMock.Verify(r => r.AddAsync(It.IsAny<TicketingSystem.Domain.Entities.Sector>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_EventNotFound_ShouldThrowException()
    {
        // Arrange
        var command = new CreateSectorCommand { EventId = 99, Name = "VIP" };
        _eventRepositoryMock.Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TicketingSystem.Domain.Entities.Event?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Event not found");
    }
}
