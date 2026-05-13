using FluentAssertions;
using Moq;
using TicketingSystem.Application.Interfaces.persistence;
using TicketingSystem.Application.Interfaces.Services;
using TicketingSystem.Application.UseCases.Sector.UpdateSector;
using TicketingSystem.Domain.Entities;
using TicketingSystem.Domain.Exceptions;
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
        var command = new UpdateSectorCommand { Id = 1, Name = "New Name", Price = 75, Capacity = 150 };
        var sector = new TicketingSystem.Domain.Entities.Sector 
        { 
            Id = 1, 
            Name = "Old Name", 
            Price = 50,
            Capacity = 100,
            EventId = 1,
            Event = new TicketingSystem.Domain.Entities.Event { Name = "E1", Status = "A" } 
        };

        _sectorRepositoryMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sector);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        sector.Name.Should().Be("New Name");
        sector.Price.Should().Be(75);
        sector.Capacity.Should().Be(150);
        _sectorRepositoryMock.Verify(r => r.UpdateAsync(sector, It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _auditRepositoryMock.Verify(a => a.AddAsync(It.IsAny<AuditLog>(), It.IsAny<CancellationToken>()), Times.Once);
        _cacheServiceMock.Verify(c => c.RemoveAsync($"Sector:{command.Id}", It.IsAny<CancellationToken>()), Times.Once);
        _cacheServiceMock.Verify(c => c.RemoveByPrefixAsync("Sectors:List", It.IsAny<CancellationToken>()), Times.Once);
        _cacheServiceMock.Verify(c => c.RemoveByPrefixAsync("Seats:List", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("", 100, 50.0, "Sector name cannot be empty.")]
    [InlineData("VIP", 100, -1.0, "Sector price cannot be negative.")]
    [InlineData("VIP", 0, 50.0, "Sector capacity must be greater than zero.")]
    public async Task Handle_InvalidInput_ShouldThrowBusinessException(string name, int? capacity, double? price, string expectedMessage)
    {
        // Arrange
        var command = new UpdateSectorCommand { Id = 1, Name = name, Capacity = capacity, Price = (decimal?)price };

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BusinessException>().WithMessage(expectedMessage);
    }

    [Fact]
    public async Task Handle_SectorNotFound_ShouldThrowException()
    {
        // Arrange
        var command = new UpdateSectorCommand { Id = 99 };
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
        var command = new UpdateSectorCommand { Id = 1, Name = "Error" };
        var sector = new TicketingSystem.Domain.Entities.Sector { Id = 1, Name = "S1", Event = new TicketingSystem.Domain.Entities.Event { Name = "E1", Status = "A" } };
        _sectorRepositoryMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sector);
        _sectorRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<TicketingSystem.Domain.Entities.Sector>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>();
        _uowMock.Verify(u => u.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
