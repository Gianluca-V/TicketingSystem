using FluentAssertions;
using Moq;
using TicketingSystem.Application.Interfaces.persistence;
using TicketingSystem.Application.Interfaces.Services;
using TicketingSystem.Application.UseCases.Sector.UpdateSector;
using TicketingSystem.Domain.Entities;
using TicketingSystem.UnitTests.Helpers;
using Xunit;

namespace TicketingSystem.UnitTests.UseCases.Sector;

public class UpdateSectorHandlerTests
{
    private readonly Mock<ISectorRepository> _sectorRepositoryMock;
    private readonly Mock<IAuditRepository> _auditRepositoryMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly UpdateSectorHandler _handler;

    public UpdateSectorHandlerTests()
    {
        _sectorRepositoryMock = new Mock<ISectorRepository>();
        _auditRepositoryMock = new Mock<IAuditRepository>();
        _cacheServiceMock = MockHelpers.MockCacheService();
        _uowMock = MockHelpers.MockUnitOfWork();

        _handler = new UpdateSectorHandler(
            _sectorRepositoryMock.Object,
            _auditRepositoryMock.Object,
            _cacheServiceMock.Object,
            _uowMock.Object
        );
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldUpdateSector()
    {
        // Arrange
        var command = new UpdateSectorCommand { Id = 1, Name = "New Name" };
        var sector = new TicketingSystem.Domain.Entities.Sector 
        { 
            Id = 1, 
            Name = "Old Name", 
            Event = new TicketingSystem.Domain.Entities.Event { Name = "E1", Status = "A" } 
        };

        _sectorRepositoryMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sector);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        sector.Name.Should().Be("New Name");
        _sectorRepositoryMock.Verify(r => r.UpdateAsync(sector, It.IsAny<CancellationToken>()), Times.Once);
    }
}
