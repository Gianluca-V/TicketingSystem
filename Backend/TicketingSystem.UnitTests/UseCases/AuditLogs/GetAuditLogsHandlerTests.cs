using FluentAssertions;
using Moq;
using TicketingSystem.Application.DTOs;
using TicketingSystem.Application.Interfaces.persistence;
using TicketingSystem.Application.Interfaces.Services;
using TicketingSystem.Application.UseCases.AuditLogs;
using TicketingSystem.Domain.Entities;
using TicketingSystem.Domain.QueryFilters;
using TicketingSystem.UnitTests.Helpers;
using Xunit;

namespace TicketingSystem.UnitTests.UseCases.AuditLogs;

public class GetAuditLogsHandlerTests
{
    private readonly Mock<IAuditRepository> _auditRepositoryMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly GetAuditLogsHandler _handler;

    public GetAuditLogsHandlerTests()
    {
        _auditRepositoryMock = new Mock<IAuditRepository>();
        _cacheServiceMock = MockHelpers.MockCacheService();
        _handler = new GetAuditLogsHandler(_auditRepositoryMock.Object, _cacheServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WhenCacheExists_ShouldReturnCachedData()
    {
        // Arrange
        var query = new GetAuditLogsQuery();
        var cachedLogs = new List<AuditLogDto> 
        { 
            new AuditLogDto(Guid.NewGuid(), 1, "Created", "User", "1", "Details", DateTime.UtcNow) 
        };

        _cacheServiceMock.Setup(c => c.GetAsync<IEnumerable<AuditLogDto>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedLogs);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(cachedLogs);
        _auditRepositoryMock.Verify(r => r.GetAllAsync(It.IsAny<AuditFilter>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenCacheMiss_ShouldFetchFromRepositoryAndCache()
    {
        // Arrange
        var query = new GetAuditLogsQuery { UserId = 1, Action = AuditAction.Created };
        var logs = new List<AuditLog>
        {
            new AuditLog { Id = Guid.NewGuid(), UserId = 1, Action = AuditAction.Created, ResourceType = "User", ResourceId = "1", Details = "Details", OccurredAt = DateTime.UtcNow }
        };

        _cacheServiceMock.Setup(c => c.GetAsync<IEnumerable<AuditLogDto>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IEnumerable<AuditLogDto>?)null);

        _auditRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<AuditFilter>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(logs);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        _cacheServiceMock.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<IEnumerable<AuditLogDto>>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithDateFilters_ShouldSetUtcKind()
    {
        // Arrange
        var fromDate = new DateTime(2023, 1, 1);
        var toDate = new DateTime(2023, 12, 31);
        var query = new GetAuditLogsQuery { From = fromDate, To = toDate };

        _cacheServiceMock.Setup(c => c.GetAsync<IEnumerable<AuditLogDto>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IEnumerable<AuditLogDto>?)null);
        
        _auditRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<AuditFilter>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AuditLog>());

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        query.From?.Kind.Should().Be(DateTimeKind.Utc);
        query.To?.Kind.Should().Be(DateTimeKind.Utc);
    }
}
