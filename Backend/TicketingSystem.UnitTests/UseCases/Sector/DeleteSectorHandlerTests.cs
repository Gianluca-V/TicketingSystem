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
        _auditRepositoryMock.Verify(a => a.AddAsync(It.IsAny<AuditLog>(), It.IsAny<CancellationToken>()), Times.Once);
        _cacheServiceMock.Verify(c => c.RemoveAsync($"Sector:{command.Id}", It.IsAny<CancellationToken>()), Times.Once);
        _cacheServiceMock.Verify(c => c.RemoveByPrefixAsync("Sectors:List", It.IsAny<CancellationToken>()), Times.Once);
        _cacheServiceMock.Verify(c => c.RemoveByPrefixAsync("Seats:List", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_SectorNotFound_ShouldThrowException()
    {
        // Arrange
        var command = new DeleteSectorCommand { Id = 99 };
        _sectorRepositoryMock.Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TicketingSystem.Domain.Entities.Sector?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Sector not found");
        _uowMock.Verify(u => u.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_GenericException_ShouldRollback()
    {
        // Arrange
        var command = new DeleteSectorCommand { Id = 1 };
        _sectorRepositoryMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>();
        _uowMock.Verify(u => u.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
