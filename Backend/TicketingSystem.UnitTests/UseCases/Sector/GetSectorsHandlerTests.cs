using FluentAssertions;
using Moq;
using TicketingSystem.Application.DTOs;
using TicketingSystem.Application.Interfaces.persistence;
using TicketingSystem.Application.Interfaces.Services;
using TicketingSystem.Application.UseCases.Sector.GetSectors;
using TicketingSystem.Domain.Entities;
using TicketingSystem.Domain.QueryFilters;
using TicketingSystem.UnitTests.Helpers;
using Xunit;

namespace TicketingSystem.UnitTests.UseCases.Sector;

public class GetSectorsHandlerTests
{
    private readonly Mock<ISectorRepository> _sectorRepositoryMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly GetSectorsHandler _handler;

    public GetSectorsHandlerTests()
    {
        _sectorRepositoryMock = new Mock<ISectorRepository>();
        _cacheServiceMock = MockHelpers.MockCacheService();
        _handler = new GetSectorsHandler(_sectorRepositoryMock.Object, _cacheServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WhenSectorsExist_ShouldReturnDtos()
    {
        // Arrange
        var sectors = new List<TicketingSystem.Domain.Entities.Sector>
        {
            new() { Id = 1, Name = "S1", Event = new() { Name = "E1" } }
        };

        _cacheServiceMock.Setup(c => c.GetAsync<IEnumerable<SectorDto>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IEnumerable<SectorDto>?)null);

        _sectorRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<SectorFilter>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(sectors);

        // Act
        var result = await _handler.Handle(new GetSectorsQuery(), CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("S1");
    }
}
