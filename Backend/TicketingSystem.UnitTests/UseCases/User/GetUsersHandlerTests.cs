using FluentAssertions;
using Moq;
using TicketingSystem.Application.DTOs;
using TicketingSystem.Application.Interfaces.persistence;
using TicketingSystem.Application.Interfaces.Services;
using TicketingSystem.Application.UseCases.User.GetUsers;
using TicketingSystem.Domain.Entities;
using TicketingSystem.Domain.QueryFilters;
using TicketingSystem.UnitTests.Helpers;
using Xunit;

namespace TicketingSystem.UnitTests.UseCases.User;

public class GetUsersHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly GetUsersHandler _handler;

    public GetUsersHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _cacheServiceMock = MockHelpers.MockCacheService();
        _handler = new GetUsersHandler(_userRepositoryMock.Object, _cacheServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WhenCacheExists_ShouldReturnCachedUsers()
    {
        // Arrange
        var cachedUsers = new List<UserDto>
        {
            new(1, "User 1", "u1@e.com"),
            new(2, "User 2", "u2@e.com")
        };

        _cacheServiceMock.Setup(c => c.GetAsync<IEnumerable<UserDto>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedUsers);

        // Act
        var result = await _handler.Handle(new GetUsersQuery(), CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(cachedUsers);
        _userRepositoryMock.Verify(r => r.GetAllAsync(It.IsAny<UserFilter>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenCacheMiss_ShouldFetchFromRepositoryAndCache()
    {
        // Arrange
        var users = new List<TicketingSystem.Domain.Entities.User>
        {
            new() { Id = 1, Name = "User 1", Email = "u1@e.com", PasswordHash = "h" }
        };

        _cacheServiceMock.Setup(c => c.GetAsync<IEnumerable<UserDto>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IEnumerable<UserDto>?)null);

        _userRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<UserFilter>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        // Act
        var result = await _handler.Handle(new GetUsersQuery(), CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        _cacheServiceMock.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<IEnumerable<UserDto>>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
