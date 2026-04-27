using FluentAssertions;
using Moq;
using TicketingSystem.Application.Interfaces.persistence;
using TicketingSystem.Application.UseCases.Sector.GetSectors;
using TicketingSystem.Domain.Entities;
using Xunit;

namespace TicketingSystem.UnitTests.UseCases.Sector;

public class GetSectorByIdHandlerTests
{
    private readonly Mock<ISectorRepository> _sectorRepositoryMock;
    private readonly GetSectorByIdHandler _handler;

    public GetSectorByIdHandlerTests()
    {
        _sectorRepositoryMock = new Mock<ISectorRepository>();
        _handler = new GetSectorByIdHandler(_sectorRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WhenSectorExistsAndBelongsToEvent_ShouldReturnDto()
    {
        // Arrange
        var sector = new TicketingSystem.Domain.Entities.Sector
        {
            Id = 10,
            EventId = 5,
            Name = "VIP",
            Price = 2500,
            Capacity = 100,
            Event = new TicketingSystem.Domain.Entities.Event
            {
                Id = 5,
                Name = "Rock Fest"
            }
        };

        _sectorRepositoryMock
            .Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sector);

        var query = new GetSectorByIdQuery
        {
            EventId = 5,
            SectorId = 10
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(10);
        result.EventId.Should().Be(5);
        result.Name.Should().Be("VIP");
        result.EventName.Should().Be("Rock Fest");
        result.Price.Should().Be(2500);
        result.Capacity.Should().Be(100);
    }

    [Fact]
    public async Task Handle_WhenSectorDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        _sectorRepositoryMock
            .Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TicketingSystem.Domain.Entities.Sector?)null);

        var query = new GetSectorByIdQuery
        {
            EventId = 5,
            SectorId = 99
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenSectorBelongsToAnotherEvent_ShouldReturnNull()
    {
        // Arrange
        var sector = new TicketingSystem.Domain.Entities.Sector
        {
            Id = 10,
            EventId = 999,
            Name = "VIP",
            Price = 2500,
            Capacity = 100,
            Event = new TicketingSystem.Domain.Entities.Event
            {
                Id = 999,
                Name = "Otro Evento"
            }
        };

        _sectorRepositoryMock
            .Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sector);

        var query = new GetSectorByIdQuery
        {
            EventId = 5,
            SectorId = 10
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }
}