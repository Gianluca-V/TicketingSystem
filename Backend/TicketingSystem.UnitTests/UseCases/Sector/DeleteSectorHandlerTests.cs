using FluentAssertions;
using Moq;
using TicketingSystem.Application.Interfaces.persistence;
using TicketingSystem.Application.Interfaces.Services;
using TicketingSystem.Application.UseCases.Sector.DeleteSector;
using TicketingSystem.Domain.Entities;
using TicketingSystem.UnitTests.Helpers;
using Xunit;

namespace TicketingSystem.UnitTests.UseCases.Sector;

public class DeleteSectorHandlerTests
{
    private readonly Mock<ISectorRepository> _sectorRepositoryMock;
    private readonly Mock<IAuditRepository> _auditRepositoryMock;
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly DeleteSectorHandler _handler;

    public DeleteSectorHandlerTests()
    {
        _sectorRepositoryMock = new Mock<ISectorRepository>();
        _auditRepositoryMock = new Mock<IAuditRepository>();
        _uowMock = MockHelpers.MockUnitOfWork();
        _cacheServiceMock = MockHelpers.MockCacheService();

        _handler = new DeleteSectorHandler(
            _sectorRepositoryMock.Object,
            _auditRepositoryMock.Object,
            _cacheServiceMock.Object,
            _uowMock.Object
        );
    }

    [Fact]
    public async Task Handle_ValidId_ShouldDeleteSector()
    {
        // Arrange
        var command = new DeleteSectorCommand { Id = 1 };
        var sector = new TicketingSystem.Domain.Entities.Sector 
        { 
            Id = 1, 
            Name = "S1", 
            Event = new TicketingSystem.Domain.Entities.Event { Name = "E1", Status = "A" } 
        };

        _sectorRepositoryMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sector);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _sectorRepositoryMock.Verify(r => r.DeleteAsync(1, It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
