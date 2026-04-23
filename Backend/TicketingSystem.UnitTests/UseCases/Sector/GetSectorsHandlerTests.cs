using FluentAssertions;
using Moq;
using TicketingSystem.Application.DTOs;
using TicketingSystem.Application.Interfaces.persistence;
using TicketingSystem.Application.UseCases.Sector.GetSectors;
using TicketingSystem.Domain.Entities;
using TicketingSystem.Domain.QueryFilters;
using Xunit;

namespace TicketingSystem.UnitTests.UseCases.Sector;

public class GetSectorsHandlerTests
{
    private readonly Mock<ISectorRepository> _sectorRepositoryMock;
    private readonly GetSectorsHandler _handler;

    public GetSectorsHandlerTests()
    {
        _sectorRepositoryMock = new Mock<ISectorRepository>();
        _handler = new GetSectorsHandler(_sectorRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WhenSectorsExist_ShouldReturnDtos()
    {
        // Arrange
        var sectors = new List<TicketingSystem.Domain.Entities.Sector>
        {
            new() { Id = 1, Name = "S1", Event = new() { Name = "E1" } }
        };

        _sectorRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<SectorFilter>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(sectors);

        // Act
        var result = await _handler.Handle(new GetSectorsQuery(), CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("S1");
    }
}
